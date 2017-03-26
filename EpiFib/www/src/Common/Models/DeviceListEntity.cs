// <copyright file="DeviceListEntity.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace Common.Models
{
    using Microsoft.WindowsAzure.Storage.Table;

    public class DeviceListEntity : TableEntity
    {
        public DeviceListEntity(string hostName, string deviceId)
        {
            this.PartitionKey = deviceId;
            this.RowKey = hostName;
        }

        public DeviceListEntity()
        {
        }

        [IgnoreProperty]
        public string HostName
        {
            get { return this.RowKey; }
            set { this.RowKey = value; }
        }

        [IgnoreProperty]
        public string DeviceId
        {
            get { return this.PartitionKey; }
            set { this.PartitionKey = value; }
        }

        public string Key { get; set; }
    }
}
