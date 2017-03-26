// <copyright file="DeviceController.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using Common.Configuration;
    using Common.Exceptions;
    using Common.Helpers;
    using Common.Models;
    using IoTInfrastructure.Exceptions;
    using IoTInfrastructure.Models;
    using IoTInfrastructure.Services;
    using Resources;
    using Web.Helpers;
    using Web.Models;
    using Web.Security;

    [Authorize]
    [OutputCache(CacheProfile = "NoCacheProfile")]
    public class DeviceController : Controller
    {
        private readonly IDeviceService deviceLogic;
        private readonly IDeviceTypeService deviceTypeLogic;
        private readonly string iotHubName;

        public DeviceController(
            IDeviceService deviceLogic, 
            IDeviceTypeService deviceTypeLogic,
            IConfigurationProvider configProvider)
        {
            this.deviceLogic = deviceLogic;
            this.deviceTypeLogic = deviceTypeLogic;
            this.iotHubName = configProvider.GetConfigurationSettingValue("iotHub.HostName");
        }

        [RequirePermission(Permission.ViewDevices)]
        public ActionResult Index()
        {
            return this.View();
        }

        [RequirePermission(Permission.AddDevices)]
        public async Task<ActionResult> AddDevice()
        {
            var deviceTypes = await this.deviceTypeLogic.GetAllDeviceTypesAsync();
            return this.View(deviceTypes);
        }

        [RequirePermission(Permission.AddDevices)]
        public async Task<ActionResult> SelectType(DeviceType deviceType)
        {
            return await Task.Run(() =>
            {
                ViewBag.CanHaveIccid = false;

                var device = new UnregisteredDeviceViewModel
                {
                    DeviceType = deviceType,
                    IsDeviceIdSystemGenerated = true
                };

                return this.PartialView("_AddDeviceCreate", device);
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(Permission.AddDevices)]
        public async Task<ActionResult> AddDeviceCreate(string button, UnregisteredDeviceViewModel model)
        {
            bool isModelValid = ModelState.IsValid;
            bool validateOnly = button != null && button.ToLower().Trim() == "check";

            if (object.ReferenceEquals(null, model) ||
                (model.GetType() == typeof(object)))
            {
                model = new UnregisteredDeviceViewModel();
            }

            // reset flag
            model.IsDeviceIdUnique = false;

            if (model.IsDeviceIdSystemGenerated)
            {
                // clear the model state of errors prior to modifying the model
                ModelState.Clear();

                // assign a system generated device Id
                model.DeviceId = Guid.NewGuid().ToString();

                // validate the model
                isModelValid = this.TryValidateModel(model);
            }

            if (isModelValid)
            {
                bool deviceExists = await this.GetDeviceExistsAsync(model.DeviceId);

                model.IsDeviceIdUnique = !deviceExists;

                if (model.IsDeviceIdUnique)
                {
                    if (!validateOnly)
                    {
                        return await this.Add(model);
                    }
                }
                else
                {
                    ModelState.AddModelError("DeviceId", Strings.DeviceIdInUse);
                }
            }

            return this.PartialView("_AddDeviceCreate", model);
        }

        [RequirePermission(Permission.ViewDevices)]
        public async Task<ActionResult> GetDeviceDetails(string deviceId)
        {
            var device = await this.deviceLogic.GetDeviceAsync(deviceId);
            if (device == null)
            {
                throw new InvalidOperationException("Unable to load device with deviceId " + deviceId);
            }

            if (device.DeviceProperties == null)
            {
                throw new DeviceRequiredPropertyNotFoundException("'DeviceProperties' property is missing");
            }

            DeviceDetailViewModel deviceModel = new DeviceDetailViewModel
            {
                DeviceID = deviceId,
                HubEnabledState = device.DeviceProperties.GetHubEnabledState(),
                DevicePropertyValueModels = new List<DevicePropertyValueModel>()
            };

            var propModels = this.deviceLogic.ExtractDevicePropertyValuesModels(device);
            propModels = ApplyDevicePropertyOrdering(propModels);

            deviceModel.DevicePropertyValueModels.AddRange(propModels);

            // check if value is cellular by checking iccid property
            deviceModel.IsCellular = device.SystemProperties.ICCID != null;
            deviceModel.Iccid = device.SystemProperties.ICCID;

            return this.PartialView("_DeviceDetails", deviceModel);
        }

        [RequirePermission(Permission.ViewDeviceSecurityKeys)]
        public async Task<ActionResult> GetDeviceKeys(string deviceId)
        {
            var keys = await this.deviceLogic.GetIoTHubKeysAsync(deviceId);

            var keysModel = new SecurityKeysViewModel
            {
                PrimaryKey = keys != null ? keys.PrimaryKey : Strings.DeviceNotRegisteredInIoTHub,
                SecondaryKey = keys != null ? keys.SecondaryKey : Strings.DeviceNotRegisteredInIoTHub
            };

            return this.PartialView("_DeviceDetailsKeys", keysModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(Permission.AddDevices)]
        public async Task<ActionResult> EditDeviceProperties(EditDevicePropertiesViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    return await this.Edit(model);
                }
                catch (ValidationException exception)
                {
                    if (exception.Errors != null && exception.Errors.Any())
                    {
                        exception.Errors.ToList().ForEach(error => ModelState.AddModelError(string.Empty, error));
                    }
                }
                catch (Exception)
                {
                    ModelState.AddModelError(string.Empty, Strings.DeviceUpdateError);
                }
            }

            return this.View("EditDeviceProperties", model);
        }

        [RequirePermission(Permission.EditDeviceMetadata)]
        public async Task<ActionResult> EditDeviceProperties(string deviceId)
        {
            var model = new EditDevicePropertiesViewModel
            {
                DevicePropertyValueModels = new List<DevicePropertyValueModel>()
            };

            var device = await this.deviceLogic.GetDeviceAsync(deviceId);
            if (device != null)
            {
                if (device.DeviceProperties == null)
                {
                    throw new DeviceRequiredPropertyNotFoundException("Required DeviceProperties not found");
                }

                model.DeviceId = device.DeviceProperties.DeviceID;
                var propValModels = this.deviceLogic.ExtractDevicePropertyValuesModels(device);
                propValModels = ApplyDevicePropertyOrdering(propValModels);

                model.DevicePropertyValueModels.AddRange(propValModels);
            }

            return this.View("EditDeviceProperties", model);
        }

        [RequirePermission(Permission.RemoveDevices)]
        public ActionResult RemoveDevice(string deviceId)
        {
            var device = new RegisteredDeviceViewModel
            {
                HostName = this.iotHubName,
                DeviceId = deviceId
            };

            return this.View("RemoveDevice", device);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(Permission.RemoveDevices)]
        public async Task<ActionResult> DeleteDevice(string deviceId)
        {
            await this.deviceLogic.RemoveDeviceAsync(deviceId);
            return this.View("Index");
        }

        private static IEnumerable<DevicePropertyValueModel> ApplyDevicePropertyOrdering(IEnumerable<DevicePropertyValueModel> devicePropertyModels)
        {
            EFGuard.NotNull(devicePropertyModels, nameof(devicePropertyModels));

            return devicePropertyModels.OrderByDescending(
                t => DeviceDisplayHelper.GetIsCopyControlPropertyName(
                    t.Name)).ThenBy(u => u.DisplayOrder).ThenBy(
                        v => v.Name);
        }

        private async Task<bool> GetDeviceExistsAsync(string deviceId)
        {
            DeviceModel existingDevice = await this.deviceLogic.GetDeviceAsync(deviceId);
            return existingDevice != null;
        }

        private async Task<ActionResult> Add(UnregisteredDeviceViewModel model)
        {
            var deviceWithKeys = await this.AddDeviceAsync(model);
            var newDevice = new RegisteredDeviceViewModel
            {
                HostName = this.iotHubName,
                DeviceType = model.DeviceType,
                DeviceId = deviceWithKeys.Device.DeviceProperties.DeviceID,
                PrimaryKey = deviceWithKeys.SecurityKeys.PrimaryKey,
                SecondaryKey = deviceWithKeys.SecurityKeys.SecondaryKey,
                InstructionsUrl = model.DeviceType.InstructionsUrl
            };

            return this.PartialView("_AddDeviceCopy", newDevice);
        }

        private async Task<DeviceWithKeys> AddDeviceAsync(UnregisteredDeviceViewModel unregisteredDeviceModel)
        {
            EFGuard.NotNull(unregisteredDeviceModel, nameof(unregisteredDeviceModel));
            EFGuard.NotNull(unregisteredDeviceModel.DeviceType, nameof(unregisteredDeviceModel.DeviceType));

            DeviceModel device = DeviceCreatorHelper.BuildDeviceStructure(
                unregisteredDeviceModel.DeviceId,
                unregisteredDeviceModel.DeviceType.IsSimulatedDevice, 
                unregisteredDeviceModel.Iccid);

            DeviceWithKeys addedDevice = await this.deviceLogic.AddDeviceAsync(device);
            return addedDevice;
        }

        private async Task<ActionResult> Edit(EditDevicePropertiesViewModel model)
        {
            if (model != null)
            {
                var device = await this.deviceLogic.GetDeviceAsync(model.DeviceId);
                if (device != null)
                {
                    this.deviceLogic.ApplyDevicePropertyValueModels(device, model.DevicePropertyValueModels);
                    await this.deviceLogic.UpdateDeviceAsync(device);
                }
            }

            return this.RedirectToAction("Index");
        }
    }
}