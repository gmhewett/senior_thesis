// <copyright file="ITransport.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace Simulator.WebJob.SimulatorCore.Transport
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Client = Microsoft.Azure.Devices.Client;

    public interface ITransport
    {
        void Open();

        Task CloseAsync();

        Task SendEventAsync(dynamic eventData);

        Task SendEventAsync(Guid eventId, dynamic eventData);

        Task SendEventBatchAsync(IEnumerable<Client.Message> messages);

        Task<DeserializableCommand> ReceiveAsync();

        Task SignalAbandonedCommand(DeserializableCommand command);

        Task SignalCompletedCommand(DeserializableCommand command);

        Task SignalRejectedCommand(DeserializableCommand command);
    }
}
