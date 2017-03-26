// <copyright file="DiagnosticTelemetryCommandProcessor.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace Simulator.WebJob.SimulatorCore.CommandProcessors
{
    using System;
    using System.Threading.Tasks;
    using Common.Helpers;
    using Common.Models;
    using Simulator.WebJob.Container.Devices;
    using Simulator.WebJob.SimulatorCore.Devices;
    using Simulator.WebJob.SimulatorCore.Transport;

    public class DiagnosticTelemetryCommandProcessor : CommandProcessor
    {
        private const string DiagnosticTelemetry = "DiagnosticTelemetry";

        public DiagnosticTelemetryCommandProcessor(IDevice device) : base(device)
        {
        }

        public override async Task<CommandProcessingResult> HandleCommandAsync(DeserializableCommand deserializableCommand)
        {
            if (deserializableCommand.CommandName == DiagnosticTelemetry)
            {
                CommandHistory commandHistory = deserializableCommand.CommandHistory;
                try
                {
                    var device = Device as ContainerDevice;
                    if (device != null)
                    {
                        dynamic parameters = commandHistory.Parameters;
                        if (parameters != null)
                        {
                            dynamic activeAsDynamic =
                                ReflectionHelper.GetNamedPropertyValue(
                                    parameters,
                                    "Active",
                                    usesCaseSensitivePropertyNameMatch: true,
                                    exceptionThrownIfNoMatch: true);

                            if (activeAsDynamic != null)
                            {
                                var active = Convert.ToBoolean(activeAsDynamic.ToString());

                                if (active != null)
                                {
                                    device.DiagnosticTelemetry(active);
                                    return CommandProcessingResult.Success;
                                }

                                return CommandProcessingResult.CannotComplete;
                            }

                            return CommandProcessingResult.CannotComplete;
                        }

                        return CommandProcessingResult.CannotComplete;
                    }
                }
                catch (Exception)
                {
                    return CommandProcessingResult.RetryLater;
                }
            }
            else if (this.NextCommandProcessor != null)
            {
                return await this.NextCommandProcessor.HandleCommandAsync(deserializableCommand);
            }

            return CommandProcessingResult.CannotComplete;
        }
    }
}
