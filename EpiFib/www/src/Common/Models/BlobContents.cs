// <copyright file="BlobContents.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace Common.Models
{
    using System;
    using System.IO;

    public class BlobContents
    {
        public Stream Data { get; set; }

        public DateTime? LastModifiedTime { get; set; }
    }
}
