// <copyright file="IBlobStorageReader.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace Common.Helpers
{
    using System.Collections.Generic;
    using Common.Models;

    public interface IBlobStorageReader : IEnumerable<BlobContents>
    {
    }
}
