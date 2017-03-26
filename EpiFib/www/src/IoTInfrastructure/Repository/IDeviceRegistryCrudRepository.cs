// <copyright file="IDeviceRegistryCrudRepository.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace IoTInfrastructure.Repository
{
    using System.Threading.Tasks;
    using Common.Models;
    using IoTInfrastructure.Models;

    public interface IDeviceRegistryCrudRepository
    {
        Task<DeviceModel> GetDeviceAsync(string deviceId);

        Task<DeviceModel> AddDeviceAsync(DeviceModel device);

        Task<DeviceModel> UpdateDeviceAsync(DeviceModel device);

        Task RemoveDeviceAsync(string deviceId);

        Task<DeviceModel> UpdateDeviceEnabledStatusAsync(string deviceId, bool isEnabled);
    }
}