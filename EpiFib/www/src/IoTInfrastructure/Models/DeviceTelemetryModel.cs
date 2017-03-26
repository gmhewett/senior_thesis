// <copyright file="DeviceTelemetryModel.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace IoTInfrastructure.Models
{
    using System;
    using System.Collections.Generic;

    public class DeviceTelemetryModel
    {
        public string DeviceId { get; set; }

        public IDictionary<string, double> Values { get; set; } = new Dictionary<string, double>();

        public DateTime? Timestamp { get; set; }
    }
}
