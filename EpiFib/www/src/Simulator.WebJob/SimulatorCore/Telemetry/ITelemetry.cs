// <copyright file="ITelemetry.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace Simulator.WebJob.SimulatorCore.Telemetry
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface ITelemetry
    {
        Task SendEventsAsync(CancellationToken token, Func<object, Task> sendMessageAsync);
    }
}
