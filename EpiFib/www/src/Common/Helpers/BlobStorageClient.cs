// <copyright file="BlobStorageClient.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace Common.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;

    public class BlobStorageClient : IBlobStorageClient
    {
        private readonly CloudBlobClient blobClient;
        private readonly string containerName;
        private CloudBlobContainer container;

        public BlobStorageClient(string connectionString, string containerName)
        {
            EFGuard.NotNull(connectionString, nameof(connectionString));
            EFGuard.NotNull(containerName, nameof(containerName));

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            this.blobClient = storageAccount.CreateCloudBlobClient();
            this.containerName = containerName;
        }

        public async Task<byte[]> GetBlobData(string blobName)
        {
            CloudBlockBlob blob = await this.CreateCloudBlockBlobAsync(blobName);
            bool exists = await blob.ExistsAsync();
            if (exists)
            {
                await blob.FetchAttributesAsync();
                var blobLength = blob.Properties.Length;

                if (blobLength > 0)
                {
                    var existingBytes = new byte[blobLength];
                    await blob.DownloadToByteArrayAsync(existingBytes, 0);
                    return existingBytes;
                }
            }

            return null;
        }

        public async Task<string> GetBlobEtag(string blobName)
        {
            var blob = await this.CreateCloudBlockBlobAsync(blobName);
            return blob.Properties.ETag;
        }

        public async Task<IBlobStorageReader> GetReader(string blobPrefix, DateTime? minTime = null)
        {
            await this.CreateCloudBlobContainerAsync();

            var blobs = await this.LoadBlobItemsAsync(async token => await this.container.ListBlobsSegmentedAsync(
                blobPrefix,
                true,
                BlobListingDetails.None,
                null,
                token,
                null,
                null));

            if (blobs != null)
            {
                blobs = blobs.OrderByDescending(this.ExtractBlobItemDate);
                if (minTime != null)
                {
                    blobs = blobs.Where(t => this.FilterLessThanTime(t, minTime.Value));
                }
            }

            return new BlobStorageReader(blobs);
        }

        public async Task UploadTextAsync(string blobName, string data)
        {
            var blob = await this.CreateCloudBlockBlobAsync(blobName);
            await blob.UploadTextAsync(data);
        }

        public async Task UploadFromByteArrayAsync(
            string blobName, 
            byte[] buffer, 
            int index, 
            int count, 
            AccessCondition accessCondition,
            BlobRequestOptions options, 
            OperationContext operationContext)
        {
            var blob = await this.CreateCloudBlockBlobAsync(blobName);

            await blob.UploadFromByteArrayAsync(
                buffer,
                index,
                count,
                accessCondition,
                options,
                operationContext);
        }

        private async Task CreateCloudBlobContainerAsync()
        {
            if (this.container == null && this.containerName != null)
            {
                this.container = this.blobClient.GetContainerReference(this.containerName);
                await this.container.CreateIfNotExistsAsync();
            }
        }

        private async Task<CloudBlockBlob> CreateCloudBlockBlobAsync(string blobName)
        {
            CloudBlockBlob blob;
            if (blobName != null)
            {
                await this.CreateCloudBlobContainerAsync();
                blob = this.container.GetBlockBlobReference(blobName);
                return blob;
            }

            return null;
        }

        private async Task<IEnumerable<IListBlobItem>> LoadBlobItemsAsync(
            Func<BlobContinuationToken, Task<BlobResultSegment>> segmentLoader)
        {
            EFGuard.NotNull(segmentLoader, nameof(segmentLoader));

            IEnumerable<IListBlobItem> blobItems = new IListBlobItem[0];

            var segment = await segmentLoader(null);
            while (segment?.Results != null)
            {
                blobItems = blobItems.Concat(segment.Results);

                if (segment.ContinuationToken == null)
                {
                    break;
                }

                segment = await segmentLoader(segment.ContinuationToken);
            }

            return blobItems;
        }

        private DateTime? ExtractBlobItemDate(IListBlobItem blobItem)
        {
            EFGuard.NotNull(blobItem, nameof(blobItem));

            BlobProperties blobProperties;
            CloudBlockBlob blockBlob;
            CloudPageBlob pageBlob;

            if ((blockBlob = blobItem as CloudBlockBlob) != null)
            {
                blobProperties = blockBlob.Properties;
            }
            else if ((pageBlob = blobItem as CloudPageBlob) != null)
            {
                blobProperties = pageBlob.Properties;
            }
            else
            {
                blobProperties = null;
            }

            if ((blobProperties != null) &&
                blobProperties.LastModified.HasValue)
            {
                return blobProperties.LastModified.Value.DateTime;
            }

            return null;
        }

        private bool FilterLessThanTime(IListBlobItem blobItem, DateTime minTime)
        {
            CloudBlockBlob blockBlob;
            if ((blockBlob = blobItem as CloudBlockBlob) != null)
            {
                if (blockBlob.Properties?.LastModified != null &&
                    (blockBlob.Properties.LastModified.Value.LocalDateTime >= minTime))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
