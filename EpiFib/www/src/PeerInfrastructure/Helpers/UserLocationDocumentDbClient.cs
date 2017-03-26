// <copyright file="UserLocationDocumentDbClient.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace PeerInfrastructure.Helpers
{
    using System.Linq;
    using System.Threading.Tasks;
    using Common.Configuration;
    using Common.Helpers;

    public class UserLocationDocumentDbClient<T> : DocumentDbClientBase<T>, IUserLocationDocumentDbClient<T> where T : new()
    {
        private readonly string databaseId;
        private readonly string collectionName;

        public UserLocationDocumentDbClient(IConfigurationProvider configProvider) : base(configProvider)
        {
            EFGuard.NotNull(configProvider, nameof(configProvider));

            this.databaseId = configProvider.GetConfigurationSettingValue("docDb.UserLocationDatabaseId");
            this.collectionName = configProvider.GetConfigurationSettingValue("docDb.UserLocationCollectionName");
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
