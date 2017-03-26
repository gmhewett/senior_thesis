// <copyright file="IoTHubTransportFactory.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace Simulator.WebJob.SimulatorCore.Transport
{
    using Common.Configuration;
    using Simulator.WebJob.SimulatorCore.Devices;
    using Simulator.WebJob.SimulatorCore.Logging;

    public class IoTHubTransportFactory : ITransportFactory
    {
        private readonly ILogger logger;
        private readonly IConfigurationProvider configurationProvider;

        public IoTHubTransportFactory(ILogger logger, IConfigurationProvider configurationProvider)
        {
            this.logger = logger;
            this.configurationProvider = configurationProvider;
        }

        public ITransport CreateTransport(IDevice device)
        {
            return new IoTHubTransport(this.logger, this.configurationProvider, device);
        }
    }
}
