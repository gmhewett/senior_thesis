// <copyright file="EmergencyInstanceDocumentDbClient.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace PeerInfrastructure.Helpers
{
    using System.Linq;
    using System.Threading.Tasks;
    using Common.Configuration;
    using Common.Helpers;

    public class EmergencyInstanceDocumentDbClient<T> : DocumentDbClientBase<T>, IEmergencyInstanceDocumentDbClient<T> where T : new()
    {
        private readonly string databaseId;
        private readonly string collectionName;

        public EmergencyInstanceDocumentDbClient(IConfigurationProvider configProvider) : base(configProvider)
        {
            EFGuard.NotNull(configProvider, nameof(configProvider));

            this.databaseId = configProvider.GetConfigurationSettingValue("docDb.EmergencyInstanceDatabaseId");
            this.collectionName = configProvider.GetConfigurationSettingValue("docDb.EmergencyInstanceCollectionName");
        }

        public Task<T> GetAsync(string id, string partitionKey)
        {
            return base.GetAsync(id, partitionKey, this.databaseId, this.collectionName);
        }

        public Task<IQueryable<T>> QueryAsync()
        {
            return base.QueryAsync(this.databaseId, this.collectionName);
        }

        public Task<T> SaveAsync(T data)
        {
            return base.SaveAsync(data, this.databaseId, this.collectionName);
        }

        public Task DeleteAsync(string id, string partitionKey)
        {
            return base.DeleteAsync(id, partitionKey, this.databaseId, this.collectionName);
        }  
    }
}