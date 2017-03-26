// <copyright file="AlertHistoryItemModel.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace IoTInfrastructure.Models
{
    using System;

    public class AlertHistoryItemModel
    {
        public string DeviceId { get; set; }

        public string RuleOutput { get; set; }

        public DateTime? Timestamp { get; set; }

        public string Value { get; set; }
    }
}
