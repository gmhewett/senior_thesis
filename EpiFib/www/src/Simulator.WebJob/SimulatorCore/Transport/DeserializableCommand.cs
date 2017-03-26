// <copyright file="DeserializableCommand.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace Simulator.WebJob.SimulatorCore.Transport
{
    using System;
    using System.Diagnostics;
    using System.Text;
    using Common.Helpers;
    using Common.Models;
    using Newtonsoft.Json;

    public class DeserializableCommand
    {
        public DeserializableCommand(CommandHistory history, string lockToken)
        {
            this.CommandHistory = history;
            this.LockToken = lockToken;
        }
        
        public DeserializableCommand(Microsoft.Azure.Devices.Client.Message message)
        {
            EFGuard.NotNull(message, nameof(message));

            Debug.Assert(
                !string.IsNullOrEmpty(message.LockToken),
                "message.LockToken is a null reference or empty string.");
            this.LockToken = message.LockToken;

            byte[] messageBytes = message.GetBytes(); // this needs to be saved if needed later, because it can only be read once from the original Message

            string jsonData = Encoding.UTF8.GetString(messageBytes);
            CommandHistory = JsonConvert.DeserializeObject<CommandHistory>(jsonData);
        }

        public string CommandName => CommandHistory.Name;

        public CommandHistory CommandHistory { get; }

        public string LockToken { get; }
    }
}
