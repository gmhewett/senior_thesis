// <copyright file="DeviceTelemetrySummaryModel.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace IoTInfrastructure.Models
{
    using System;

    public class DeviceTelemetrySummaryModel
    {
        public double? AverageHumidity { get; set; }

        public string DeviceId { get; set; }

        public double? MaximumHumidity { get; set; }

        public double? MinimumHumidity { get; set; }

        public double? TimeFrameMinutes { get; set; }

        public DateTime? Timestamp { get; set; }
    }
}
