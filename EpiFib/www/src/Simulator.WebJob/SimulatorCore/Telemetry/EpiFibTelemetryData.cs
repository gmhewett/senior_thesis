// <copyright file="EpiFibTelemetryData.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace Simulator.WebJob.SimulatorCore.Telemetry
{
    public class EpiFibTelemetryData
    {
        public string DeviceId { get; set; }

        public double Temperature { get; set; }

        public double Humidity { get; set; }

        public double? ExternalTemperature { get; set; }
    }
}
