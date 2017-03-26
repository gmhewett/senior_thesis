// <copyright file="TableStorageResponse.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace Common.Models
{
    public enum TableStorageResponseStatus
    {
        Successful, ConflictError, UnknownError, DuplicateInsert, NotFound
    }

    public class TableStorageResponse<T>
    {
        public T Entity { get; set; }

        public TableStorageResponseStatus Status { get; set; }
    }
}
