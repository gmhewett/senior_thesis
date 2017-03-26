// <copyright file="ContainerTelemetryFactory.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace Simulator.WebJob.Container.Telemetry
{
    using Simulator.WebJob.SimulatorCore.Devices;
    using Simulator.WebJob.SimulatorCore.Logging;
    using Simulator.WebJob.SimulatorCore.Telemetry;

    public class ContainerTelemetryFactory : ITelemetryFactory
    {
        private readonly ILogger logger;

        public ContainerTelemetryFactory(ILogger logger)
        {
            this.logger = logger;
        }

        public object PopulateDeviceWithTelemetryEvents(IDevice device)
        {
            var startupTelemetry = new StartupTelemetry(this.logger, device);
            device.TelemetryEvents.Add(startupTelemetry);

            var monitorTelemetry = new EpiFibTelemetry(this.logger, device.DeviceID);
            device.TelemetryEvents.Add(monitorTelemetry);

            return monitorTelemetry;
        }
    }
}
