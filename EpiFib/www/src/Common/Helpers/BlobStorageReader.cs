// <copyright file="BlobStorageReader.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace Common.Helpers
{
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using Common.Models;
    using Microsoft.WindowsAzure.Storage.Blob;

    internal class BlobStorageReader : IBlobStorageReader
    {
        private readonly IEnumerable<IListBlobItem> blobs;

        public BlobStorageReader(IEnumerable<IListBlobItem> blobs)
        {
            EFGuard.NotNull(blobs, nameof(blobs));
            this.blobs = blobs;
        }

        public IEnumerator<BlobContents> GetEnumerator()
        {
            foreach (var blob in this.blobs)
            {
                CloudBlockBlob blockBlob;
                if ((blockBlob = blob as CloudBlockBlob) == null)
                {
                    continue;
                }

                var stream = new MemoryStream();
                blockBlob.DownloadToStream(stream);
                yield return
                    new BlobContents
                    {
                        Data = stream,
                        LastModifiedTime = blockBlob.Properties.LastModified?.LocalDateTime
                    };
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
