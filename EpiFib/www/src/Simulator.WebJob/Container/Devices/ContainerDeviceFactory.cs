// <copyright file="ContainerDeviceFactory.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace Simulator.WebJob.Container.Devices
{
    using Common.Configuration;
    using Common.Models;
    using Simulator.WebJob.SimulatorCore.Devices;
    using Simulator.WebJob.SimulatorCore.Logging;
    using Simulator.WebJob.SimulatorCore.Telemetry;
    using Simulator.WebJob.SimulatorCore.Transport;

    public class ContainerDeviceFactory : IDeviceFactory
    {
        public IDevice CreateDevice(
            ILogger logger,
            ITransportFactory transportFactory, 
            ITelemetryFactory telemetryFactory,
            IConfigurationProvider configurationProvider, 
            InitialDeviceConfig config)
        {
            var device = new ContainerDevice(logger, transportFactory, telemetryFactory, configurationProvider);
            device.Init(config);
            return device;
        }
    }
}
