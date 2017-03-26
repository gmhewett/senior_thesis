// <copyright file="IoTHubRepository.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace IoTInfrastructure.Repository
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Common.Helpers;
    using Common.Models;
    using Microsoft.Azure.Devices;
    using Newtonsoft.Json;

    public class IoTHubRepository : IIoTHubRepository, IDisposable
    {
        private readonly IIoTHubDeviceManager deviceManager;
        private bool isDisposed;

        public IoTHubRepository(IIoTHubDeviceManager deviceManager)
        {
            EFGuard.NotNull(deviceManager, nameof(deviceManager));

            this.deviceManager = deviceManager;
        }

        ~IoTHubRepository()
        {
            this.Dispose(false);
        }

        public async Task<Device> GetIotHubDeviceAsync(string deviceId)
        {
            return await AzureRetryHelper.OperationWithBasicRetryAsync(async () =>
                await this.deviceManager.GetDeviceAsync(deviceId));
        }

        public async Task<DeviceModel> AddDeviceAsync(DeviceModel device, SecurityKeys securityKeys)
        {
            var iotHubDevice = new Device(device.DeviceProperties.DeviceID)
            {
                Status = device.DeviceProperties.HubEnabledState != null &&
                         (bool)device.DeviceProperties.HubEnabledState
                    ? DeviceStatus.Enabled
                    : DeviceStatus.Disabled
            };
            
            var authentication = new AuthenticationMechanism
            {
                SymmetricKey = new SymmetricKey
                {
                    PrimaryKey = securityKeys.PrimaryKey,
                    SecondaryKey = securityKeys.SecondaryKey
                }
            };

            iotHubDevice.Authentication = authentication;

            await AzureRetryHelper.OperationWithBasicRetryAsync(async () =>
                await this.deviceManager.AddDeviceAsync(iotHubDevice));

            return device;
        }

        public async Task<bool> TryAddDeviceAsync(Device oldIotHubDevice)
        {
            try
            {
                var newIotHubDevice = new Device(oldIotHubDevice.Id)
                {
                    Authentication = oldIotHubDevice.Authentication,
                    Status = oldIotHubDevice.Status
                };

                await AzureRetryHelper.OperationWithBasicRetryAsync(async () =>
                    await this.deviceManager.AddDeviceAsync(newIotHubDevice));
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public async Task RemoveDeviceAsync(string deviceId)
        {
            await AzureRetryHelper.OperationWithBasicRetryAsync(async () =>
                await this.deviceManager.RemoveDeviceAsync(deviceId));
        }

        public async Task<bool> TryRemoveDeviceAsync(string deviceId)
        {
            try
            {
                await AzureRetryHelper.OperationWithBasicRetryAsync(async () =>
                    await this.deviceManager.RemoveDeviceAsync(deviceId));
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public async Task<Device> UpdateDeviceEnabledStatusAsync(string deviceId, bool isEnabled)
        {
            Device iotHubDevice =
                await AzureRetryHelper.OperationWithBasicRetryAsync(async () =>
                    await this.deviceManager.GetDeviceAsync(deviceId));

            iotHubDevice.Status = isEnabled ? DeviceStatus.Enabled : DeviceStatus.Disabled;

            return await AzureRetryHelper.OperationWithBasicRetryAsync(async () =>
                await this.deviceManager.UpdateDeviceAsync(iotHubDevice));
        }

        public async Task SendCommand(string deviceId, CommandHistory command)
        {
            byte[] commandAsBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(command));
            var notificationMessage = new Message(commandAsBytes)
            {
                Ack = DeliveryAcknowledgement.Full,
                MessageId = command.MessageId
            };
            
            await AzureRetryHelper.OperationWithBasicRetryAsync(async () =>
                await this.deviceManager.SendAsync(deviceId, notificationMessage));

            await this.deviceManager.CloseAsyncDevice();
        }

        public async Task<SecurityKeys> GetDeviceKeysAsync(string deviceId)
        {
            Device iotHubDevice = await this.deviceManager.GetDeviceAsync(deviceId);

            return iotHubDevice == null
                ? null
                : new SecurityKeys(
                    iotHubDevice.Authentication.SymmetricKey.PrimaryKey,
                    iotHubDevice.Authentication.SymmetricKey.SecondaryKey);
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
                this.deviceManager?.CloseAsyncService().Wait();
            }

            this.isDisposed = true;
        }
    }
}