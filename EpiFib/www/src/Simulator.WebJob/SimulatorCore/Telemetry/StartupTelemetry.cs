// <copyright file="StartupTelemetry.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace Simulator.WebJob.Container.Telemetry
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Simulator.WebJob.SimulatorCore.Devices;
    using Simulator.WebJob.SimulatorCore.Logging;
    using Simulator.WebJob.SimulatorCore.Telemetry;

    public class StartupTelemetry : ITelemetry
    {
        private readonly ILogger logger;
        private readonly IDevice device;

        public StartupTelemetry(ILogger logger, IDevice device)
        {
            this.logger = logger;
            this.device = device;
        }

        public async Task SendEventsAsync(CancellationToken token, Func<object, Task> sendMessageAsync)
        {
            if (!token.IsCancellationRequested)
            {
                this.logger.LogInfo("Sending initial data for device {0}", this.device.DeviceID);
                await sendMessageAsync(this.device.GetDeviceInfo());
            }
        }
    }
}
