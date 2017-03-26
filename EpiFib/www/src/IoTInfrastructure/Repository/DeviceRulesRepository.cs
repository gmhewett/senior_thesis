// <copyright file="DeviceRulesRepository.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace IoTInfrastructure.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.Configuration;
    using Common.Helpers;
    using Common.Models;
    using IoTInfrastructure.Models;
    using Microsoft.WindowsAzure.Storage.Table;
    using Newtonsoft.Json;

    public class DeviceRulesRepository : IDeviceRulesRepository
    {
        private const int BlobSaveMinutesInTheFuture = 2;

        private readonly string blobName;
        private readonly IAzureTableStorageClient azureTableStorageClient;
        private readonly IBlobStorageClient blobStorageClient;
        private readonly DateTimeFormatInfo formatInfo;

        public DeviceRulesRepository(
            IConfigurationProvider configurationProvider,
            IAzureTableStorageClientFactory tableStorageClientFactory,
            IBlobStorageClientFactory blobStorageClientFactory)
        {
            EFGuard.NotNull(configurationProvider, nameof(configurationProvider));
            EFGuard.NotNull(tableStorageClientFactory, nameof(tableStorageClientFactory));
            EFGuard.NotNull(blobStorageClientFactory, nameof(blobStorageClientFactory));

            string storageAccountConnectionString = configurationProvider.GetConfigurationSettingValue("device.StorageConnectionString");
            string deviceRulesBlobContainerName = configurationProvider.GetConfigurationSettingValue("device.DeviceRulesStoreContainerName");
            string deviceRulesNormalizedTableName = configurationProvider.GetConfigurationSettingValue("device.DeviceRulesTableName");
            this.azureTableStorageClient = tableStorageClientFactory.CreateClient(storageAccountConnectionString, deviceRulesNormalizedTableName);
            this.blobName = configurationProvider.GetConfigurationSettingValue("asa.AsaRefDataRulesBlobName");
            this.blobStorageClient = blobStorageClientFactory.CreateClient(storageAccountConnectionString, deviceRulesBlobContainerName);

            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
            this.formatInfo = culture.DateTimeFormat;
            this.formatInfo.ShortDatePattern = @"yyyy-MM-dd";
            this.formatInfo.ShortTimePattern = @"HH-mm";
        }

        public async Task<List<DeviceRule>> GetAllRulesAsync()
        {
            List<DeviceRule> result = new List<DeviceRule>();

            IEnumerable<DeviceRuleTableEntity> queryResults = await this.GetAllRulesFromTable();
            foreach (DeviceRuleTableEntity rule in queryResults)
            {
                var deviceRule = this.BuildRuleFromTableEntity(rule);
                result.Add(deviceRule);
            }

            return result;
        }

        public async Task<DeviceRule> GetDeviceRuleAsync(string deviceId, string ruleId)
        {
            TableOperation query = TableOperation.Retrieve<DeviceRuleTableEntity>(deviceId, ruleId);

            TableResult response = await Task.Run(() => this.azureTableStorageClient.Execute(query));

            DeviceRule result = this.BuildRuleFromTableEntity((DeviceRuleTableEntity)response.Result);
            return result;
        }

        public async Task<List<DeviceRule>> GetAllRulesForDeviceAsync(string deviceId)
        {
            TableQuery<DeviceRuleTableEntity> query = new TableQuery<DeviceRuleTableEntity>().Where(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, deviceId));
            var devicesResult = await this.azureTableStorageClient.ExecuteQueryAsync(query);
            return devicesResult.Select(this.BuildRuleFromTableEntity).ToList();
        }

        public async Task<TableStorageResponse<DeviceRule>> SaveDeviceRuleAsync(DeviceRule updatedRule)
        {
            DeviceRuleTableEntity incomingEntity = this.BuildTableEntityFromRule(updatedRule);

            TableStorageResponse<DeviceRule> result =
                await this.azureTableStorageClient.DoTableInsertOrReplaceAsync<DeviceRule, DeviceRuleTableEntity>(
                    incomingEntity, 
                    this.BuildRuleFromTableEntity);

            if (result.Status == TableStorageResponseStatus.Successful)
            {
                // Build up a new blob to push up for ASA job ref data
                List<DeviceRuleBlobEntity> blobList = await this.BuildBlobEntityListFromTableRows();
                await this.PersistRulesToBlobStorageAsync(blobList);
            }

            return result;
        }

        public async Task<TableStorageResponse<DeviceRule>> DeleteDeviceRuleAsync(DeviceRule ruleToDelete)
        {
            DeviceRuleTableEntity incomingEntity = this.BuildTableEntityFromRule(ruleToDelete);

            TableStorageResponse<DeviceRule> result =
                await this.azureTableStorageClient.DoDeleteAsync<DeviceRule, DeviceRuleTableEntity>(
                    incomingEntity, 
                    this.BuildRuleFromTableEntity);

            if (result.Status == TableStorageResponseStatus.Successful)
            {
                List<DeviceRuleBlobEntity> blobList = await this.BuildBlobEntityListFromTableRows();
                await this.PersistRulesToBlobStorageAsync(blobList);
            }

            return result;
        }

        private async Task<IEnumerable<DeviceRuleTableEntity>> GetAllRulesFromTable()
        {
            TableQuery<DeviceRuleTableEntity> query = new TableQuery<DeviceRuleTableEntity>();

            return await this.azureTableStorageClient.ExecuteQueryAsync(query);
        }

        private DeviceRule BuildRuleFromTableEntity(DeviceRuleTableEntity tableEntity)
        {
            if (tableEntity == null)
            {
                return null;
            }

            var updatedRule = new DeviceRule(tableEntity.RuleId)
            {
                DeviceID = tableEntity.DeviceId,
                DataField = tableEntity.DataField,
                Threshold = tableEntity.Threshold,
                EnabledState = tableEntity.Enabled,
                Operator = ">",
                RuleOutput = tableEntity.RuleOutput,
                Etag = tableEntity.ETag
            };

            return updatedRule;
        }

        private DeviceRuleTableEntity BuildTableEntityFromRule(DeviceRule incomingRule)
        {
            DeviceRuleTableEntity tableEntity =
                new DeviceRuleTableEntity(incomingRule.DeviceID, incomingRule.RuleId)
                {
                    DataField = incomingRule.DataField,
                    Threshold = incomingRule.Threshold ?? 0,
                    Enabled = incomingRule.EnabledState,
                    RuleOutput = incomingRule.RuleOutput
                };

            if (!string.IsNullOrWhiteSpace(incomingRule.Etag))
            {
                tableEntity.ETag = incomingRule.Etag;
            }

            return tableEntity;
        }

        private async Task<List<DeviceRuleBlobEntity>> BuildBlobEntityListFromTableRows()
        {
            IEnumerable<DeviceRuleTableEntity> queryResults = await this.GetAllRulesFromTable();
            Dictionary<string, DeviceRuleBlobEntity> blobEntityDictionary = new Dictionary<string, DeviceRuleBlobEntity>();
            foreach (DeviceRuleTableEntity rule in queryResults)
            {
                if (rule.Enabled)
                {
                    DeviceRuleBlobEntity entity;
                    if (!blobEntityDictionary.ContainsKey(rule.PartitionKey))
                    {
                        entity = new DeviceRuleBlobEntity(rule.PartitionKey);
                        blobEntityDictionary.Add(rule.PartitionKey, entity);
                    }
                    else
                    {
                        entity = blobEntityDictionary[rule.PartitionKey];
                    }

                    if (rule.DataField == DeviceRuleDataFields.Temperature)
                    {
                        entity.Temperature = rule.Threshold;
                        entity.TemperatureRuleOutput = rule.RuleOutput;
                    }
                    else if (rule.DataField == DeviceRuleDataFields.Humidity)
                    {
                        entity.Humidity = rule.Threshold;
                        entity.HumidityRuleOutput = rule.RuleOutput;
                    }
                }
            }

            return blobEntityDictionary.Values.ToList();
        }

        private async Task PersistRulesToBlobStorageAsync(List<DeviceRuleBlobEntity> blobList)
        {
            string updatedJson = JsonConvert.SerializeObject(blobList);
            DateTime saveDate = DateTime.UtcNow.AddMinutes(BlobSaveMinutesInTheFuture);
            string dateString = saveDate.ToString("d", this.formatInfo);
            string timeString = saveDate.ToString("t", this.formatInfo);
            string formattedBlobName = $@"{dateString}\{timeString}\{this.blobName}";

            await this.blobStorageClient.UploadTextAsync(formattedBlobName, updatedJson);
        }
    }
}
