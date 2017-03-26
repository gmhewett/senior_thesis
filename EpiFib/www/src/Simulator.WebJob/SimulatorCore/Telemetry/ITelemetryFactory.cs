// <copyright file="ITelemetryFactory.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace Simulator.WebJob.SimulatorCore.Telemetry
{
    using Simulator.WebJob.SimulatorCore.Devices;

    public interface ITelemetryFactory
    {
        object PopulateDeviceWithTelemetryEvents(IDevice device);
    }
}
