// <copyright file="IAzureTableStorageClientFactory.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace Common.Helpers
{
    public interface IAzureTableStorageClientFactory
    {
        IAzureTableStorageClient CreateClient(string storageConnectionString, string tableName);
    }
}
