// <copyright file="IBlobStorageClient.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace Common.Helpers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;

    public interface IBlobStorageClient
    {
        Task<byte[]> GetBlobData(string blobName);

        Task<string> GetBlobEtag(string blobName);

        Task<IBlobStorageReader> GetReader(string blobPrefix, DateTime? minTime = null);

        Task UploadTextAsync(string blobName, string data);

        Task UploadFromByteArrayAsync(
            string blobName,
            byte[] buffer,
            int index,
            int count,
            AccessCondition accessCondition,
            BlobRequestOptions options,
            OperationContext operationContext);
    }
}
