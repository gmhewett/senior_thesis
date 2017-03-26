// <copyright file="DashboardDevicePaneDataViewModel.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace Web.Models
{
    using IoTInfrastructure.Models;

    public class DashboardDevicePaneDataViewModel
    {
        public string DeviceId { get; set; }

        public DeviceTelemetryModel[] DeviceTelemetryModels { get; set; }

        public DeviceTelemetrySummaryModel DeviceTelemetrySummaryModel { get; set; }

        public DeviceTelemetryFieldModel[] DeviceTelemetryFields { get; set; }
    }
}