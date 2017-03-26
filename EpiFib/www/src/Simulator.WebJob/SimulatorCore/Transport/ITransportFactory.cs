// <copyright file="ITransportFactory.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace Simulator.WebJob.SimulatorCore.Transport
{
    using Simulator.WebJob.SimulatorCore.Devices;

    public interface ITransportFactory
    {
        ITransport CreateTransport(IDevice device);
    }
}
