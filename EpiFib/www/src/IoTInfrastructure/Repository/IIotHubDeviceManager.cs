// <copyright file="IIoTHubDeviceManager.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace IoTInfrastructure.Repository
{
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices;

    public interface IIoTHubDeviceManager
    {
        Task<Device> AddDeviceAsync(Device device);

        Task<Device> GetDeviceAsync(string deviceId);

        Task RemoveDeviceAsync(string deviceId);

        Task<Device> UpdateDeviceAsync(Device device);

        Task SendAsync(string deviceId, Message message);

        Task CloseAsyncService();

        Task CloseAsyncDevice();
    }
}
