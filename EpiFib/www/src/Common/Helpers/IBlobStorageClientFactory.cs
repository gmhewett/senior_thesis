// <copyright file="IBlobStorageClientFactory.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace Common.Helpers
{
    public interface IBlobStorageClientFactory
    {
        IBlobStorageClient CreateClient(string storageConnectionString, string containerName);
    }
}
