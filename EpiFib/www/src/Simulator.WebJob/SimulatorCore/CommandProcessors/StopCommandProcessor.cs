// <copyright file="StopCommandProcessor.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace Simulator.WebJob.SimulatorCore.CommandProcessors
{
    using System;
    using System.Threading.Tasks;
    using Simulator.WebJob.Container.Devices;
    using Simulator.WebJob.SimulatorCore.Devices;
    using Simulator.WebJob.SimulatorCore.Transport;

    public class StopCommandProcessor : CommandProcessor
    {
        private const string StopTelemetry = "StopTelemetry";

        public StopCommandProcessor(IDevice device) : base(device)
        {
        }

        public override async Task<CommandProcessingResult> HandleCommandAsync(DeserializableCommand deserializableCommand)
        {
            if (deserializableCommand.CommandName == StopTelemetry)
            {
                try
                {
                    var device = Device as ContainerDevice;
                    if (device == null)
                    {
                        return CommandProcessingResult.CannotComplete;
                    }

                    device.StopTelemetryData();
                    return CommandProcessingResult.Success;
                }
                catch (Exception)
                {
                    return CommandProcessingResult.RetryLater;
                }
            }

            if (this.NextCommandProcessor != null)
            {
                return await this.NextCommandProcessor.HandleCommandAsync(deserializableCommand);
            }

            return CommandProcessingResult.CannotComplete;
        }
    }
}
