// <copyright file="BlobStorageClientFactory.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace Common.Helpers
{
    public class BlobStorageClientFactory : IBlobStorageClientFactory
    {
        private IBlobStorageClient blobStorageClient;

        public BlobStorageClientFactory() : this(null)
        {
        }

        public BlobStorageClientFactory(IBlobStorageClient customClient)
        {
            this.blobStorageClient = customClient;
        }

        public IBlobStorageClient CreateClient(string storageConnectionString, string containerName)
        {
            return this.blobStorageClient ??
                (this.blobStorageClient = new BlobStorageClient(storageConnectionString, containerName));
        }
    }
}
