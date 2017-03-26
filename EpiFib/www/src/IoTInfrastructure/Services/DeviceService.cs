// <copyright file="DeviceService.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace IoTInfrastructure.Services
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.ExceptionServices;
    using System.Threading.Tasks;
    using Common.Configuration;
    using Common.Exceptions;
    using Common.Factory;
    using Common.Helpers;
    using Common.Models;
    using Common.Repository;
    using IoTInfrastructure.Exceptions;
    using IoTInfrastructure.Models;
    using IoTInfrastructure.Properties;
    using IoTInfrastructure.Repository;
    using Microsoft.Azure.Devices;

    public class DeviceService : IDeviceService
    {
        private readonly IConfigurationProvider configProvider;
        private readonly IIoTHubRepository ioTHubRepository;
        private readonly IDeviceRegistryCrudRepository deviceRegistryCrudRepository;
        private readonly IDeviceRegistryListRepository deviceRegistryListRepository;
        private readonly IVirtualDeviceStorage virtualDeviceStorage;

        private readonly ISecurityKeyGenerator securityKeyGenerator;
        private readonly IDeviceRulesService deviceRulesService;

        public DeviceService(
            IConfigurationProvider configProvider,
            IIoTHubRepository ioTHubRepository,
            IDeviceRegistryCrudRepository deviceRegistryCrudRepository,
            IDeviceRegistryListRepository deviceRegistryListRepository,
            IVirtualDeviceStorage virtualDeviceStorage,
            ISecurityKeyGenerator securityKeyGenerator,
            IDeviceRulesService deviceRulesService)
        {
            EFGuard.NotNull(configProvider, nameof(configProvider));
            EFGuard.NotNull(ioTHubRepository, nameof(ioTHubRepository));
            EFGuard.NotNull(deviceRegistryCrudRepository, nameof(deviceRegistryCrudRepository));
            EFGuard.NotNull(deviceRegistryListRepository, nameof(deviceRegistryListRepository));
            EFGuard.NotNull(virtualDeviceStorage, nameof(virtualDeviceStorage));
            EFGuard.NotNull(securityKeyGenerator, nameof(securityKeyGenerator));
            EFGuard.NotNull(deviceRulesService, nameof(deviceRulesService));
            
            this.configProvider = configProvider;
            this.ioTHubRepository = ioTHubRepository;
            this.deviceRegistryCrudRepository = deviceRegistryCrudRepository;
            this.deviceRegistryListRepository = deviceRegistryListRepository;
            this.virtualDeviceStorage = virtualDeviceStorage;
            this.securityKeyGenerator = securityKeyGenerator;
            this.deviceRulesService = deviceRulesService;
        }

        public async Task<DeviceListQueryResult> GetDevices(DeviceListQuery query)
        {
            return await this.deviceRegistryListRepository.GetDeviceList(query);
        }

        public async Task<DeviceModel> GetDeviceAsync(string deviceId)
        {
            return await this.deviceRegistryCrudRepository.GetDeviceAsync(deviceId);
        }

        public async Task<DeviceWithKeys> AddDeviceAsync(DeviceModel device)
        {
            await this.ValidateDevice(device);
            SecurityKeys generatedSecurityKeys = this.securityKeyGenerator.CreateRandomKeys();
            DeviceModel savedDevice = await this.AddDeviceToRepositoriesAsync(device, generatedSecurityKeys);

            return new DeviceWithKeys(savedDevice, generatedSecurityKeys);
        }

        public async Task<DeviceModel> UpdateDeviceAsync(DeviceModel device)
        {
            return await this.deviceRegistryCrudRepository.UpdateDeviceAsync(device);
        }

        public async Task<DeviceModel> UpdateDeviceFromDeviceInfoPacketAsync(DeviceModel device)
        {
            EFGuard.NotNull(device, nameof(device));

            DeviceModel existingDevice = await this.GetDeviceAsync(device.IoTHub.ConnectionDeviceId);

            if (existingDevice.DeviceProperties != null)
            {
                DeviceProperties deviceProperties = device.DeviceProperties;
                deviceProperties.CreatedTime = existingDevice.DeviceProperties.CreatedTime;
                existingDevice.DeviceProperties = deviceProperties;
            }

            device.CommandHistory = existingDevice.CommandHistory;

            device.SystemProperties = existingDevice.SystemProperties;

            if (device.Telemetry != null)
            {
                existingDevice.Telemetry = device.Telemetry;
            }

            if (device.Commands != null)
            {
                existingDevice.Commands = device.Commands;
            }

            return await this.deviceRegistryCrudRepository.UpdateDeviceAsync(existingDevice);
        }

        public async Task<DeviceModel> UpdateDeviceEnabledStatusAsync(string deviceId, bool isEnabled)
        {
            DeviceModel repositoryDevice = null;
            ExceptionDispatchInfo capturedException = null;

            await this.ioTHubRepository.UpdateDeviceEnabledStatusAsync(deviceId, isEnabled);

            try
            {
                repositoryDevice =
                    await this.deviceRegistryCrudRepository.UpdateDeviceEnabledStatusAsync(deviceId, isEnabled);
            }
            catch (Exception ex)
            {
                capturedException = ExceptionDispatchInfo.Capture(ex);
            }

            if (capturedException != null)
            {
                await this.ioTHubRepository.UpdateDeviceEnabledStatusAsync(deviceId, !isEnabled);
                capturedException.Throw();
            }

            if (repositoryDevice == null || !repositoryDevice.IsSimulatedDevice)
            {
                return repositoryDevice;
            }

            return await this.AddOrRemoveSimulatedDevice(repositoryDevice, isEnabled);
        }

        public async Task RemoveDeviceAsync(string deviceId)
        {
            ExceptionDispatchInfo capturedException = null;
            Device iotHubDevice = await this.ioTHubRepository.GetIotHubDeviceAsync(deviceId);

            if (iotHubDevice == null)
            {
                throw new DeviceNotRegisteredException(deviceId);
            }

            await this.ioTHubRepository.RemoveDeviceAsync(deviceId);

            try
            {
                await this.deviceRegistryCrudRepository.RemoveDeviceAsync(deviceId);
            }
            catch (Exception ex)
            {
                capturedException = ExceptionDispatchInfo.Capture(ex);
            }

            if (capturedException == null)
            {
                try
                {
                    await this.virtualDeviceStorage.RemoveDeviceAsync(deviceId);
                }
                catch (Exception ex)
                {
                    Trace.TraceError($"Failed to remove simulated device : {ex.Message}");
                }

                await this.deviceRulesService.RemoveAllRulesForDeviceAsync(deviceId);
            }
            else
            {
                await this.ioTHubRepository.TryAddDeviceAsync(iotHubDevice);
                capturedException.Throw();
            }
        }

        public async Task<SecurityKeys> GetIoTHubKeysAsync(string deviceId)
        {
            return await this.ioTHubRepository.GetDeviceKeysAsync(deviceId);
        }

        public async Task GenerateNDevices(int deviceCount)
        {
            var randomNumber = new Random();

            for (int i = 0; i < deviceCount; i++)
            {
                SecurityKeys generatedSecurityKeys = this.securityKeyGenerator.CreateRandomKeys();
                DeviceModel device = SampleDeviceFactory.GetSampleDevice(randomNumber, generatedSecurityKeys);
                await this.AddDeviceToRepositoriesAsync(device, generatedSecurityKeys);
            }
        }

        public async Task SendCommandAsync(string deviceId, string commandName, dynamic parameters)
        {
            DeviceModel device = await this.GetDeviceAsync(deviceId);

            if (device == null)
            {
                throw new DeviceNotRegisteredException(deviceId);
            }

            await this.SendCommandAsyncWithDevice(device, commandName, parameters);
        }

        public async Task<List<string>> BootstrapDefaultDevices()
        {
            List<string> sampleIds = SampleDeviceFactory.GetDefaultDeviceNames();
            foreach (string id in sampleIds)
            {
                DeviceModel device = DeviceCreatorHelper.BuildDeviceStructure(id, true, null);
                SecurityKeys generatedSecurityKeys = this.securityKeyGenerator.CreateRandomKeys();
                await this.AddDeviceToRepositoriesAsync(device, generatedSecurityKeys);
            }

            return sampleIds;
        }

        public DeviceListLocationsModel ExtractLocationsData(List<DeviceModel> devices)
        {
            var result = new DeviceListLocationsModel();

            double minLat = double.MaxValue;
            double maxLat = double.MinValue;
            double minLong = double.MaxValue;
            double maxLong = double.MinValue;

            var locationList = new List<DeviceLocationModel>();
            if (devices != null && devices.Count > 0)
            {
                foreach (DeviceModel device in devices)
                {
                    if (device.DeviceProperties == null)
                    {
                        throw new DeviceRequiredPropertyNotFoundException("Required DeviceProperties not found");
                    }

                    if (device.DeviceProperties.Longitude == null || device.DeviceProperties.Latitude == null)
                    {
                        continue;
                    }

                    double latitude;
                    double longitude;

                    try
                    {
                        latitude = device.DeviceProperties.Latitude.Value;
                        longitude = device.DeviceProperties.Longitude.Value;
                    }
                    catch (FormatException)
                    {
                        continue;
                    }

                    var location = new DeviceLocationModel
                    {
                        DeviceId = device.DeviceProperties.DeviceID,
                        Longitude = longitude,
                        Latitude = latitude
                    };
                    locationList.Add(location);

                    if (longitude < minLong)
                    {
                        minLong = longitude;
                    }

                    if (longitude > maxLong)
                    {
                        maxLong = longitude;
                    }

                    if (latitude < minLat)
                    {
                        minLat = latitude;
                    }

                    if (latitude > maxLat)
                    {
                        maxLat = latitude;
                    }
                }
            }

            if (locationList.Count == 0)
            {
                minLat = 47.6;
                maxLat = 47.6;
                minLong = -122.3;
                maxLong = -122.3;
            }

            const double Offset = 0.05;

            result.DeviceLocationList = locationList;
            result.MinimumLatitude = minLat - Offset;
            result.MaximumLatitude = maxLat + Offset;
            result.MinimumLongitude = minLong - Offset;
            result.MaximumLongitude = maxLong + Offset;

            return result;
        }

        public IList<DeviceTelemetryFieldModel> ExtractTelemetry(DeviceModel device)
        {
            if (device?.Telemetry != null)
            {
                return (from field in device.Telemetry
                    let displayName = field.DisplayName
                    select new DeviceTelemetryFieldModel
                    {
                        DisplayName = displayName,
                        Name = field.Name,
                        Type = field.Type
                    }).ToList();
            }

            return null;
        }

        public IEnumerable<DevicePropertyValueModel> ExtractDevicePropertyValuesModels(DeviceModel device)
        {
            EFGuard.NotNull(device, nameof(device));

            DeviceProperties deviceProperties = device.DeviceProperties;
            if (deviceProperties == null)
            {
                throw new DeviceRequiredPropertyNotFoundException("Required DeviceProperties not found");
            }

            IEnumerable<DevicePropertyValueModel> propValModels = ExtractPropertyValueModels(deviceProperties);
            string hostNameValue = this.configProvider.GetConfigurationSettingValue("iotHub.HostName");

            if (!string.IsNullOrEmpty(hostNameValue))
            {
                propValModels = propValModels.Concat(
                    new[]
                    {
                        new DevicePropertyValueModel
                        {
                            DisplayOrder = 0,
                            IsEditable = false,
                            IsIncludedWithUnregisteredDevices = true,
                            Name = "HostName",
                            PropertyType = PropertyType.String,
                            Value = hostNameValue
                        }
                    });
            }

            return propValModels;
        }

        public void ApplyDevicePropertyValueModels(
            DeviceModel device,
            IEnumerable<DevicePropertyValueModel> devicePropertyValueModels)
        {
            EFGuard.NotNull(device, nameof(device));

            if (devicePropertyValueModels == null)
            {
                throw new ArgumentNullException(nameof(devicePropertyValueModels));
            }

            if (device.DeviceProperties == null)
            {
                throw new DeviceRequiredPropertyNotFoundException("Required DeviceProperties not found");
            }

            ApplyPropertyValueModels(device.DeviceProperties, devicePropertyValueModels);
        }
        
        private static IEnumerable<DevicePropertyValueModel> ExtractPropertyValueModels(
            DeviceProperties deviceProperties)
        {
            EFGuard.NotNull(deviceProperties, nameof(deviceProperties));

            Dictionary<string, DevicePropertyMetadata> devicePropertyIndex = GetDevicePropertyConfiguration().ToDictionary(t => t.Name);

            int editableOrdering = 1;
            int nonediableOrdering = int.MinValue;

            Type devicePropertiesType = deviceProperties.GetType();
            foreach (PropertyInfo prop in devicePropertiesType.GetProperties())
            {
                bool isDisplayedRegistered;
                bool isDisplayedUnregistered;
                bool isEditable;
                DevicePropertyMetadata propertyMetadata;
                PropertyType propertyType;
                if (devicePropertyIndex.TryGetValue(
                    prop.Name,
                    out propertyMetadata))
                {
                    isDisplayedRegistered = propertyMetadata.IsDisplayedForRegisteredDevices;
                    isDisplayedUnregistered = propertyMetadata.IsDisplayedForUnregisteredDevices;
                    isEditable = propertyMetadata.IsEditable;
                    propertyType = propertyMetadata.PropertyType;
                }
                else
                {
                    isDisplayedRegistered = isEditable = true;
                    isDisplayedUnregistered = false;
                    propertyType = PropertyType.String;
                }

                if (!isDisplayedRegistered && !isDisplayedUnregistered)
                {
                    continue;
                }

                MethodInfo getMethod;
                if ((getMethod = prop.GetGetMethod()) == null)
                {
                    continue;
                }

                if (!prop.CanWrite)
                {
                    isEditable = false;
                }

                var currentData = new DevicePropertyValueModel
                {
                    Name = prop.Name,
                    PropertyType = propertyType
                };

                if (isEditable)
                {
                    currentData.IsEditable = true;
                    currentData.DisplayOrder = editableOrdering++;
                }
                else
                {
                    currentData.IsEditable = false;
                    currentData.DisplayOrder = nonediableOrdering++;
                }

                currentData.IsIncludedWithUnregisteredDevices = isDisplayedUnregistered;

                object currentValue = getMethod.Invoke(deviceProperties, ReflectionHelper.EmptyArray);
                currentData.Value = currentValue == null
                    ? string.Empty
                    : string.Format(CultureInfo.InvariantCulture, "{0}", currentValue);

                yield return currentData;
            }
        }

        private static IEnumerable<DevicePropertyMetadata> GetDevicePropertyConfiguration()
        {
            yield return new DevicePropertyMetadata()
            {
                IsDisplayedForRegisteredDevices = true,
                IsDisplayedForUnregisteredDevices = true,
                IsEditable = false,
                Name = "DeviceID"
            };

            yield return new DevicePropertyMetadata()
            {
                IsDisplayedForRegisteredDevices = true,
                IsDisplayedForUnregisteredDevices = true,
                IsEditable = false,
                Name = "CreatedTime",
                PropertyType = PropertyType.DateTime
            };

            yield return new DevicePropertyMetadata()
            {
                IsDisplayedForRegisteredDevices = true,
                IsDisplayedForUnregisteredDevices = false,
                IsEditable = false,
                Name = "DeviceState",
                PropertyType = PropertyType.Status
            };

            yield return new DevicePropertyMetadata()
            {
                IsDisplayedForRegisteredDevices = false,
                IsDisplayedForUnregisteredDevices = false,
                IsEditable = false,
                Name = "HostName"
            };

            yield return new DevicePropertyMetadata
            {
                IsDisplayedForRegisteredDevices = true,
                IsDisplayedForUnregisteredDevices = false,
                IsEditable = false,
                Name = "HubEnabledState",
                PropertyType = PropertyType.Status
            };

            yield return new DevicePropertyMetadata()
            {
                IsDisplayedForRegisteredDevices = true,
                IsDisplayedForUnregisteredDevices = false,
                IsEditable = false,
                Name = "UpdatedTime",
                PropertyType = PropertyType.DateTime
            };

            yield return new DevicePropertyMetadata()
            {
                IsDisplayedForRegisteredDevices = true,
                IsDisplayedForUnregisteredDevices = false,
                IsEditable = true,
                Name = "Latitude",
                PropertyType = PropertyType.Real
            };

            yield return new DevicePropertyMetadata()
            {
                IsDisplayedForRegisteredDevices = true,
                IsDisplayedForUnregisteredDevices = false,
                IsEditable = true,
                Name = "Longitude",
                PropertyType = PropertyType.Real
            };
        }

        private static void ApplyPropertyValueModels(
            DeviceProperties deviceProperties,
            IEnumerable<DevicePropertyValueModel> devicePropertyValueModels)
        {
            Dictionary<string, DevicePropertyMetadata> devicePropertyIndex = GetDevicePropertyConfiguration().ToDictionary(t => t.Name);

            Type devicePropertiesType = deviceProperties.GetType();
            Dictionary<string, PropertyInfo> propIndex = devicePropertiesType.GetProperties().ToDictionary(t => t.Name);

            object[] args = new object[1];
            foreach (DevicePropertyValueModel propVal in devicePropertyValueModels)
            {
                if (string.IsNullOrEmpty(propVal?.Name))
                {
                    continue;
                }

                DevicePropertyMetadata propMetadata;
                if (devicePropertyIndex.TryGetValue(propVal.Name, out propMetadata) && !propMetadata.IsEditable)
                {
                    continue;
                }

                PropertyInfo propInfo;
                MethodInfo setter;
                if (!propIndex.TryGetValue(propVal.Name, out propInfo) || ((setter = propInfo.GetSetMethod()) == null))
                {
                    continue;
                }

                TypeConverter converter = TypeDescriptor.GetConverter(propInfo.PropertyType);

                try
                {
                    args[0] = converter.ConvertFromString(propVal.Value);
                }
                catch (NotSupportedException ex)
                {
                    throw new InvalidOperationException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "Unable to assign value, \"{0},\" to Device property, {1}.",
                            propVal.Value,
                            propInfo.Name),
                        ex);
                }

                setter.Invoke(deviceProperties, args);
            }
        }

        private async Task ValidateDevice(DeviceModel device)
        {
            List<string> validationErrors = new List<string>();

            if (this.ValidateDeviceId(device, validationErrors))
            {
                await this.CheckIfDeviceExists(device, validationErrors);
            }

            if (validationErrors.Count > 0)
            {
                var validationException =
                    new ValidationException(device.DeviceProperties?.DeviceID);

                foreach (string error in validationErrors)
                {
                    validationException.Errors.Add(error);
                }

                throw validationException;
            }
        }

        private bool ValidateDeviceId(DeviceModel device, List<string> validationErrors)
        {
            if (string.IsNullOrWhiteSpace(device.DeviceProperties?.DeviceID))
            {
                validationErrors.Add(Resources.ValidationDeviceIdMissing);
                return false;
            }

            return true;
        }

        private async Task CheckIfDeviceExists(DeviceModel device, List<string> validationErrors)
        {
            if (await this.GetDeviceAsync(device.DeviceProperties.DeviceID) != null)
            {
                validationErrors.Add(Resources.ValidationDeviceExists);
            }
        }

        private async Task<DeviceModel> AddDeviceToRepositoriesAsync(DeviceModel device, SecurityKeys securityKeys)
        {
            DeviceModel registryRepositoryDevice = null;
            ExceptionDispatchInfo capturedException = null;

            await this.ioTHubRepository.AddDeviceAsync(device, securityKeys);

            try
            {
                registryRepositoryDevice = await this.deviceRegistryCrudRepository.AddDeviceAsync(device);
            }
            catch (Exception ex)
            {
                capturedException = ExceptionDispatchInfo.Capture(ex);
            }

            if (capturedException == null)
            {
                if (device.IsSimulatedDevice)
                {
                    try
                    {
                        await this.virtualDeviceStorage.AddOrUpdateDeviceAsync(new InitialDeviceConfig
                        {
                            DeviceId = device.DeviceProperties.DeviceID,
                            HostName = this.configProvider.GetConfigurationSettingValue("ioTHub.HostName"),
                            Key = securityKeys.PrimaryKey
                        });
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError($"Failed to add simulated device : {ex.Message}");
                    }
                }
            }
            else
            {
                await this.ioTHubRepository.TryRemoveDeviceAsync(device.DeviceProperties.DeviceID);
                capturedException.Throw();
            }

            return registryRepositoryDevice;
        }

        private async Task<DeviceModel> AddOrRemoveSimulatedDevice(DeviceModel repositoryDevice, bool isEnabled)
        {
            string deviceId = repositoryDevice.DeviceProperties.DeviceID;
            if (isEnabled)
            {
                try
                {
                    SecurityKeys securityKeys = await this.GetIoTHubKeysAsync(deviceId);
                    await this.virtualDeviceStorage.AddOrUpdateDeviceAsync(new InitialDeviceConfig
                    {
                        DeviceId = deviceId,
                        HostName = this.configProvider.GetConfigurationSettingValue("iotHub.HostName"),
                        Key = securityKeys.PrimaryKey
                    });
                }
                catch (Exception ex)
                {
                    Trace.TraceError(
                        $"Failed to add enabled device to simulated device storage. Device telemetry is expected not to be sent. : {ex.Message}");
                }
            }
            else
            {
                try
                {
                    await this.virtualDeviceStorage.RemoveDeviceAsync(deviceId);
                }
                catch (Exception ex)
                {
                    Trace.TraceError(
                        $"Failed to remove disabled device from simulated device store. Device will keep sending telemetry data. : {ex.Message}");
                }
            }

            return repositoryDevice;
        }

        private async Task<CommandHistory> SendCommandAsyncWithDevice(
            DeviceModel device, 
            string commandName,
            dynamic parameters)
        {
            EFGuard.NotNull(device, nameof(device));

            string deviceId = device.DeviceProperties.DeviceID;
            if (device.Commands.FirstOrDefault(x => x.Name == commandName) == null)
            {
                throw new UnsupportedCommandException(deviceId, commandName);
            }

            var commandHistory = new CommandHistory(commandName, parameters);

            if (device.CommandHistory == null)
            {
                device.CommandHistory = new List<CommandHistory>();
            }

            device.CommandHistory.Add(commandHistory);

            await this.ioTHubRepository.SendCommand(deviceId, commandHistory);
            await this.deviceRegistryCrudRepository.UpdateDeviceAsync(device);

            return commandHistory;
        }
    }
}
