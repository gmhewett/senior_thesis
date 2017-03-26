// <copyright file="IoTHubDeviceManager.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace IoTInfrastructure.Repository
{
    using System;
    using System.Threading.Tasks;
    using Common.Configuration;
    using Microsoft.Azure.Devices;

    public class IoTHubDeviceManager : IIoTHubDeviceManager, IDisposable
    {
        private readonly RegistryManager deviceManager;
        private readonly ServiceClient serviceClient;
        private bool isDisposed;

        public IoTHubDeviceManager(IConfigurationProvider configProvider)
        {
            string iotHubConnectionString = configProvider.GetConfigurationSettingValue("ioTHub.ConnectionString");
            this.deviceManager = RegistryManager.CreateFromConnectionString(iotHubConnectionString);
            this.serviceClient = ServiceClient.CreateFromConnectionString(iotHubConnectionString);
        }

        ~IoTHubDeviceManager()
        {
            this.Dispose(false);
        }

        public async Task<Device> AddDeviceAsync(Device device)
        {
            return await this.deviceManager.AddDeviceAsync(device);
        }

        public async Task<Device> GetDeviceAsync(string deviceId)
        {
            return await this.deviceManager.GetDeviceAsync(deviceId);
        }

        public async Task RemoveDeviceAsync(string deviceId)
        {
            await this.deviceManager.RemoveDeviceAsync(deviceId);
        }

        public async Task<Device> UpdateDeviceAsync(Device device)
        {
            return await this.deviceManager.UpdateDeviceAsync(device);
        }

        public async Task SendAsync(string deviceId, Message message)
        {
            await this.serviceClient.SendAsync(deviceId, message);
        }

        public async Task CloseAsyncService()
        {
            await this.serviceClient.CloseAsync();
        }

        public async Task CloseAsyncDevice()
        {
            await this.deviceManager.CloseAsync();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                this.deviceManager?.CloseAsync().Wait();
            }

            this.isDisposed = true;
        }
    }
}
