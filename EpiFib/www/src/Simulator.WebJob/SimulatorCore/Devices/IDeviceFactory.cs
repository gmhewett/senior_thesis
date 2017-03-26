// <copyright file="IDeviceFactory.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace Simulator.WebJob.SimulatorCore.Devices
{
    using Common.Configuration;
    using Common.Models;
    using Simulator.WebJob.SimulatorCore.Telemetry;
    using Simulator.WebJob.SimulatorCore.Transport;

    public interface IDeviceFactory
    {
        IDevice CreateDevice(
            Logging.ILogger logger, 
            ITransportFactory transportFactory,
            ITelemetryFactory telemetryFactory,
            IConfigurationProvider configurationProvider, 
            InitialDeviceConfig config);
    }
}
