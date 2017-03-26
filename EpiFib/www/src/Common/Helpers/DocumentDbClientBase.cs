// <copyright file="DocumentDbClientBase.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace Common.Helpers
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.Configuration;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Newtonsoft.Json;

    public class DocumentDbClientBase<T> : IDocumentDbClientBase<T> where T : new()
    {
        private readonly DocumentClient client;

        public DocumentDbClientBase(IConfigurationProvider configProvider)
        {
            EFGuard.NotNull(configProvider, nameof(configProvider));

            string endpointUrl = configProvider.GetConfigurationSettingValue("docDb.EndpointUrl");
            string primaryAuthorizationKey = configProvider.GetConfigurationSettingValue("docDb.PrimaryAuthorizationKey");

            this.client = new DocumentClient(new Uri(endpointUrl), primaryAuthorizationKey);
        }

        public async Task<T> GetAsync(string id, string partitionKey, string databaseId, string collectionName)
        {
            ResourceResponse<Document> response = await this.client.ReadDocumentAsync(
                UriFactory.CreateDocumentUri(databaseId, collectionName, id), 
                new RequestOptions
                {
                    PartitionKey = new PartitionKey(partitionKey)
                });
            return await Deserialize(response.Resource);
        }

        public async Task<IQueryable<T>> QueryAsync(string databaseId, string collectionName)
        {
            await Task.Delay(0);
            return this.client.CreateDocumentQuery<T>(
                UriFactory.CreateDocumentCollectionUri(databaseId, collectionName),
                new FeedOptions { EnableCrossPartitionQuery = true });
        }

        public async Task<T> SaveAsync(T data, string databaseId, string collectionName)
        {
            var response = await this.client.UpsertDocumentAsync(UriFactory.CreateDocumentCollectionUri(databaseId, collectionName), data);
            return await Deserialize(response.Resource);
        }

        public async Task DeleteAsync(string id, string partitionKey, string databaseId, string collectionName)
        {
            await this.client.DeleteDocumentAsync(
                UriFactory.CreateDocumentUri(databaseId, collectionName, id), 
                new RequestOptions
                {
                    PartitionKey = new PartitionKey(partitionKey)
                });
        }
        
        private static async Task<T> Deserialize(Document document)
        {
            using (var documentStream = new MemoryStream())
            using (var reader = new StreamReader(documentStream))
            {
                document.SaveTo(documentStream);
                documentStream.Position = 0;
                string rawDocumentData = await reader.ReadToEndAsync();
                return JsonConvert.DeserializeObject<T>(rawDocumentData);
            }
        }
    }
}
