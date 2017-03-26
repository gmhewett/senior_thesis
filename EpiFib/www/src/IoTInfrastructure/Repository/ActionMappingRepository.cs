// <copyright file="ActionMappingRepository.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace IoTInfrastructure.Repository
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Common.Configuration;
    using Common.Helpers;
    using IoTInfrastructure.Models;
    using Microsoft.WindowsAzure.Storage;
    using Newtonsoft.Json;

    public class ActionMappingRepository : IActionMappingRepository
    {
        private readonly IBlobStorageClient blobStorageManager;
        private readonly string blobName;

        public ActionMappingRepository(IConfigurationProvider configurationProvider, IBlobStorageClientFactory blobStorageClientFactory)
        {
            this.blobName = configurationProvider.GetConfigurationSettingValue("action.ActionMappingStoreBlobName");
            string connectionString = configurationProvider.GetConfigurationSettingValue("device.StorageConnectionString");
            string containerName = configurationProvider.GetConfigurationSettingValue("action.ActionMappingStoreContainerName");
            this.blobStorageManager = blobStorageClientFactory.CreateClient(connectionString, containerName);
        }

        public async Task<List<ActionMapping>> GetAllMappingsAsync()
        {
            ActionMappingBlobResults results = await this.GetActionsAndEtagAsync();
            return results.ActionMappings;
        }

        public async Task SaveMappingAsync(ActionMapping m)
        {
            ActionMappingBlobResults existingResults = await this.GetActionsAndEtagAsync();
            List<ActionMapping> existingMappings = existingResults.ActionMappings;
            ActionMapping found = existingMappings.FirstOrDefault(a => a.RuleOutput.ToLower() == m.RuleOutput.ToLower());

            if (found == null)
            {
                existingMappings.Add(m);
            }
            else
            {
                found.ActionId = m.ActionId;
            }

            string newJsonData = JsonConvert.SerializeObject(existingMappings);
            byte[] newBytes = Encoding.UTF8.GetBytes(newJsonData);

            await this.blobStorageManager.UploadFromByteArrayAsync(
                this.blobName,
                newBytes,
                0,
                newBytes.Length,
                AccessCondition.GenerateIfMatchCondition(existingResults.ETag),
                null,
                null);
        }

        private async Task<ActionMappingBlobResults> GetActionsAndEtagAsync()
        {
            var mappings = new List<ActionMapping>();
            byte[] blobData = await this.blobStorageManager.GetBlobData(this.blobName);

            if (blobData != null && blobData.Length > 0)
            {
                string existingJsonData = Encoding.UTF8.GetString(blobData);
                mappings = JsonConvert.DeserializeObject<List<ActionMapping>>(existingJsonData);
                string etag = await this.blobStorageManager.GetBlobEtag(this.blobName);
                return new ActionMappingBlobResults(mappings, etag);
            }

            return new ActionMappingBlobResults(mappings, string.Empty);
        }

        private class ActionMappingBlobResults
        {
            public ActionMappingBlobResults(List<ActionMapping> actionMappings, string eTag)
            {
                this.ActionMappings = actionMappings;
                this.ETag = eTag;
            }

            public List<ActionMapping> ActionMappings { get; private set; }

            public string ETag { get; private set; }
        }
    }
}
