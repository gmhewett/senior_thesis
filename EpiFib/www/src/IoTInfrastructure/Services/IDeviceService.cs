// <copyright file="IDeviceService.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace IoTInfrastructure.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Common.Models;
    using IoTInfrastructure.Models;

    public interface IDeviceService
    {
        Task<DeviceListQueryResult> GetDevices(DeviceListQuery query);

        Task<DeviceModel> GetDeviceAsync(string deviceId);

        Task<DeviceWithKeys> AddDeviceAsync(DeviceModel device);

        Task<DeviceModel> UpdateDeviceAsync(DeviceModel device);

        Task<DeviceModel> UpdateDeviceFromDeviceInfoPacketAsync(DeviceModel device);

        Task<DeviceModel> UpdateDeviceEnabledStatusAsync(string deviceId, bool isEnabled);

        Task RemoveDeviceAsync(string deviceId);

        Task<SecurityKeys> GetIoTHubKeysAsync(string deviceId);

        Task GenerateNDevices(int deviceCount);

        Task SendCommandAsync(string deviceId, string commandName, dynamic parameters);

        Task<List<string>> BootstrapDefaultDevices();

        DeviceListLocationsModel ExtractLocationsData(List<DeviceModel> devices);

        IList<DeviceTelemetryFieldModel> ExtractTelemetry(DeviceModel device);

        IEnumerable<DevicePropertyValueModel> ExtractDevicePropertyValuesModels(DeviceModel device);

        void ApplyDevicePropertyValueModels(DeviceModel device, IEnumerable<DevicePropertyValueModel> devicePropertyValueModels);
    }
}
