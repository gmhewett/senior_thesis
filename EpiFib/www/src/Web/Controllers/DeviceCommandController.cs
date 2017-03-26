// <copyright file="DeviceCommandController.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using Common.Exceptions;
    using Common.Helpers;
    using Common.Models;
    using IoTInfrastructure.Services;
    using Newtonsoft.Json;
    using Web.Models;
    using Web.Security;

    [Authorize]
    [OutputCache(CacheProfile = "NoCacheProfile")]
    public class DeviceCommandController : Controller
    {
        // ReSharper disable once CollectionNeverUpdated.Local
        private static readonly List<string> PrivateCommands = new List<string>();

        private readonly IDeviceService deviceService;
        private readonly ICommandParameterTypeService commandParameterTypeService;

        public DeviceCommandController(IDeviceService deviceService, ICommandParameterTypeService commandParameterTypeService)
        {
            this.deviceService = deviceService;
            this.commandParameterTypeService = commandParameterTypeService;
        }

        [RequirePermission(Permission.ViewDevices)]
        public async Task<ActionResult> Index(string deviceId)
        {
            DeviceModel device = await this.deviceService.GetDeviceAsync(deviceId);
            if (device.DeviceProperties == null)
            {
                throw new DeviceRequiredPropertyNotFoundException("'DeviceProperties' property is missing");
            }

            IList<SelectListItem> commandListItems = this.CommandListItems(device);

            bool deviceIsEnabled = device.DeviceProperties.GetHubEnabledState();

            DeviceCommandViewModel deviceCommandsModel = new DeviceCommandViewModel
            {
                CommandHistory = device.CommandHistory,
                CommandsJson = JsonConvert.SerializeObject(device.Commands),
                SendCommandModel = new SendCommandViewModel
                {
                    DeviceId = device.DeviceProperties.DeviceID,
                    CommandSelectList = commandListItems,
                    CanSendDeviceCommands = deviceIsEnabled && PermsChecker.HasPermission(Permission.SendCommandToDevices)
                },
                DeviceId = device.DeviceProperties.DeviceID
            };

            return this.View(deviceCommandsModel);
        }

        [HttpPost]
        [RequirePermission(Permission.SendCommandToDevices)]
        [ValidateAntiForgeryToken]
        public ActionResult Command(string deviceId, Command command)
        {
            var model = new CommandViewModel
            {
                DeviceId = deviceId,
                Name = command.Name,
                Parameters = command.Parameters.ToParametersModel().ToList()
            };
            return this.PartialView("_SendCommandForm", model);
        }

        [HttpPost]
        [RequirePermission(Permission.SendCommandToDevices)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCommand(CommandViewModel model)
        {
            if (ModelState.IsValid)
            {
                IDictionary<string, object> parameters = new Dictionary<string, object>();

                if (model.Parameters != null)
                {
                    foreach (var parameter in model.Parameters)
                    {
                        parameters.Add(new KeyValuePair<string, object>(
                            parameter.Name,
                            this.commandParameterTypeService.Get(parameter.Type, parameter.Value)));
                    }
                }

                await this.deviceService.SendCommandAsync(model.DeviceId, model.Name, parameters);

                return this.Json(new { data = model });
            }

            return this.PartialView("_SendCommandForm", model);
        }

        [HttpPost]
        [RequirePermission(Permission.SendCommandToDevices)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResendCommand(string deviceId, string name, string commandJson)
        {
            try
            {
                IDictionary<string, object> commandParameters = JsonConvert.DeserializeObject<Dictionary<string, object>>(commandJson);

                await this.deviceService.SendCommandAsync(deviceId, name, commandParameters);
            }
            catch
            {
                return this.Json(new { error = "Failed to send device" });
            }

            return this.Json(new { wasSent = true });
        }

        private static bool IsCommandPublic(Command command)
        {
            EFGuard.NotNull(command, nameof(command));

            if (command.Name == null)
            {
                throw new DeviceRequiredPropertyNotFoundException("'Name' property on command not found");
            }

            return !PrivateCommands.Contains(command.Name);
        }

        private IList<SelectListItem> CommandListItems(DeviceModel device)
        {
            return device.Commands != null ? this.GetCommandListItems(device) : new List<SelectListItem>();
        }

        private IList<SelectListItem> GetCommandListItems(DeviceModel device)
        {
            IList<SelectListItem> result = new List<SelectListItem>();
            IList<Command> commands = device.Commands;

            if (commands != null)
            {
                foreach (Command command in commands)
                {
                    if (IsCommandPublic(command))
                    {
                        SelectListItem item = new SelectListItem();
                        item.Value = command.Name;
                        item.Text = command.Name;
                        result.Add(item);
                    }
                }
            }

            return result;
        }
    }
}