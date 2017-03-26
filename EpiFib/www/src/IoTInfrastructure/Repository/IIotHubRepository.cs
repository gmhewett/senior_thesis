// <copyright file="IIoTHubRepository.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace IoTInfrastructure.Repository
{
    using System.Threading.Tasks;
    using Common.Models;
    using Microsoft.Azure.Devices;

    public interface IIoTHubRepository
    {
        Task<Device> GetIotHubDeviceAsync(string deviceId);

        Task<DeviceModel> AddDeviceAsync(DeviceModel device, SecurityKeys securityKeys);

        Task<bool> TryAddDeviceAsync(Device oldIotHubDevice);

        Task RemoveDeviceAsync(string deviceId);

        Task<bool> TryRemoveDeviceAsync(string deviceId);

        Task<Device> UpdateDeviceEnabledStatusAsync(string deviceId, bool isEnabled);

        Task SendCommand(string deviceId, CommandHistory command);

        Task<SecurityKeys> GetDeviceKeysAsync(string deviceId);
    }
}
