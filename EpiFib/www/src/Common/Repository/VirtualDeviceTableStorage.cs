// <copyright file="VirtualDeviceTableStorage.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace Common.Repository
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.Configuration;
    using Common.Helpers;
    using Common.Models;
    using Microsoft.WindowsAzure.Storage.Table;

    public class VirtualDeviceTableStorage : IVirtualDeviceStorage
    {
        private readonly IAzureTableStorageClient azureTableStorageClient;

        public VirtualDeviceTableStorage(
            IConfigurationProvider configProvider, 
            IAzureTableStorageClientFactory tableStorageClientFactory)
        {
            string storageConnectionString = configProvider.GetConfigurationSettingValue("device.StorageConnectionString");
            string deviceTableName = configProvider.GetConfigurationSettingValue("device.SimulatorTableName");
            this.azureTableStorageClient = tableStorageClientFactory.CreateClient(storageConnectionString, deviceTableName);
        }

        public async Task<List<InitialDeviceConfig>> GetDeviceListAsync()
        {
            var query = new TableQuery<DeviceListEntity>();
            IEnumerable<DeviceListEntity> devicesResult = await this.azureTableStorageClient.ExecuteQueryAsync(query);
            return devicesResult.Select(device => 
                new InitialDeviceConfig
                {
                    HostName = device.HostName,
                    DeviceId = device.DeviceId,
                    Key = device.Key
                }).ToList();
        }

        public Task<InitialDeviceConfig> GetDeviceAsync(string deviceId)
        {
            var query =
                new TableQuery<DeviceListEntity>().Where(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, deviceId));
            return this.GetDeviceAsync(query);
        }

        public async Task AddOrUpdateDeviceAsync(InitialDeviceConfig deviceConfig)
        {
            DeviceListEntity deviceEnity = new DeviceListEntity
            {
                DeviceId = deviceConfig.DeviceId,
                HostName = deviceConfig.HostName,
                Key = deviceConfig.Key
            };

            var operation = TableOperation.InsertOrReplace(deviceEnity);
            await this.azureTableStorageClient.ExecuteAsync(operation);
        }

        public async Task<bool> RemoveDeviceAsync(string deviceId)
        {
            InitialDeviceConfig device = await this.GetDeviceAsync(deviceId);
            if (device != null)
            {
                var operation = TableOperation.Retrieve<DeviceListEntity>(device.DeviceId, device.HostName);
                TableResult result = await this.azureTableStorageClient.ExecuteAsync(operation);

                DeviceListEntity deleteDevice = (DeviceListEntity)result.Result;
                if (deleteDevice != null)
                {
                    var deleteOperation = TableOperation.Delete(deleteDevice);
                    await this.azureTableStorageClient.ExecuteAsync(deleteOperation);
                    return true;
                }
            }

            return false;
        }

        private async Task<InitialDeviceConfig> GetDeviceAsync(TableQuery<DeviceListEntity> query)
        {
            IEnumerable<DeviceListEntity> devicesResult = await this.azureTableStorageClient.ExecuteQueryAsync(query);
            return devicesResult.Select(device => 
                new InitialDeviceConfig
                {
                    DeviceId = device.DeviceId,
                    HostName = device.HostName,
                    Key = device.Key
                }).FirstOrDefault();
        }
    }
}
