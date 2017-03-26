// <copyright file="DeviceRule.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace IoTInfrastructure.Models
{
    using System;

    public class DeviceRule
    {
        public DeviceRule()
        {
        }

        public DeviceRule(string ruleId)
        {
            this.RuleId = ruleId;
        }

        public string RuleId { get; set; }

        public bool EnabledState { get; set; }

        public string DeviceID { get; set; }

        public string DataField { get; set; }

        public string Operator { get; set; }

        public double? Threshold { get; set; }

        public string RuleOutput { get; set; }

        public string Etag { get; set; }

        public void InitializeNewRule(string deviceId)
        {
            this.DeviceID = deviceId;
            this.RuleId = Guid.NewGuid().ToString();
            this.EnabledState = true;
            this.Operator = ">";
        }
    }
}
