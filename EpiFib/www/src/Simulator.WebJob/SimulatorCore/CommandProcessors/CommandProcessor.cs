// <copyright file="CommandProcessor.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace Simulator.WebJob.SimulatorCore.CommandProcessors
{
    using System.Threading.Tasks;
    using Simulator.WebJob.SimulatorCore.Devices;
    using Simulator.WebJob.SimulatorCore.Transport;

    public enum CommandProcessingResult
    {
        Success = 0,
        RetryLater,
        CannotComplete
    }

    public abstract class CommandProcessor
    {
        protected CommandProcessor(IDevice device)
        {
            this.Device = device;
        }

        public IDevice Device { get; set; }

        public CommandProcessor NextCommandProcessor { get; set; }

        public abstract Task<CommandProcessingResult> HandleCommandAsync(DeserializableCommand message);
    }
}
