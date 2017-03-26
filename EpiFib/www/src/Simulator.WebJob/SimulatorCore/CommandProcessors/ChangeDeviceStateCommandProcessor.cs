// <copyright file="ChangeDeviceStateCommandProcessor.cs" company="The Reach Lab, LLC">
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

    public class ChangeDeviceStateCommandProcessor : CommandProcessor
    {
        private const string ChangeDeviceState = "ChangeDeviceState";

        public ChangeDeviceStateCommandProcessor(IDevice device) : base(device)
        {
        }

        public override async Task<CommandProcessingResult> HandleCommandAsync(DeserializableCommand deserializableCommand)
        {
            if (deserializableCommand.CommandName == ChangeDeviceState)
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
                            dynamic deviceState = ReflectionHelper.GetNamedPropertyValue(
                                parameters,
                                "DeviceState",
                                usesCaseSensitivePropertyNameMatch: true,
                                exceptionThrownIfNoMatch: true);

                            if (deviceState != null)
                            {
                                await device.ChangeDeviceState(deviceState.ToString());

                                return CommandProcessingResult.Success;
                            }

                            return CommandProcessingResult.CannotComplete;
                        }

                        return CommandProcessingResult.CannotComplete;
                    }

                    return CommandProcessingResult.CannotComplete;
                }
                catch (Exception)
                {
                    return CommandProcessingResult.RetryLater;
                }
            }

            if (this.NextCommandProcessor != null)
            {
                return await NextCommandProcessor.HandleCommandAsync(deserializableCommand);
            }

            return CommandProcessingResult.CannotComplete;
        }
    }
}
