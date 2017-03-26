// <copyright file="DeviceRuleTableEntity.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace Common.Models
{
    using Microsoft.WindowsAzure.Storage.Table;

    public class DeviceRuleTableEntity : TableEntity
    {
        public DeviceRuleTableEntity()
        {
        }

        public DeviceRuleTableEntity(string deviceId, string ruleId)
        {
            this.PartitionKey = deviceId;
            this.RowKey = ruleId;
        }
        
        [IgnoreProperty]
        public string DeviceId
        {
            get { return this.PartitionKey; }
            set { this.PartitionKey = value; }
        }

        [IgnoreProperty]
        public string RuleId
        {
            get { return this.RowKey; }
            set { this.RowKey = value; }
        }

        public string DataField { get; set; }

        public double Threshold { get; set; }

        public string RuleOutput { get; set; }

        public bool Enabled { get; set; }

        public string RuleName { get; set; }
    }
}
