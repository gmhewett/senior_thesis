// <copyright file="AzureTableStorageClientFactory.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace Common.Helpers
{
    public class AzureTableStorageClientFactory : IAzureTableStorageClientFactory
    {
        private IAzureTableStorageClient tableStorageClient;

        public AzureTableStorageClientFactory() : this(null)
        {
        }

        public AzureTableStorageClientFactory(IAzureTableStorageClient customClient)
        {
            this.tableStorageClient = customClient;
        }

        public IAzureTableStorageClient CreateClient(string storageConnectionString, string tableName)
        {
            return this.tableStorageClient ??
                   (this.tableStorageClient = new AzureTableStorageClient(storageConnectionString, tableName));
        }
    }
}
