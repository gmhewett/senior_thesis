// <copyright file="ActionProcessorFactory.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace EventProcessor.WebJob.Processors
{
    using System;
    using System.Collections.Concurrent;
    using Common.Configuration;
    using IoTInfrastructure.Services;
    using Microsoft.ServiceBus.Messaging;

    public class ActionProcessorFactory : IEventProcessorFactory
    {
        private readonly IActionService actionService;
        private readonly IActionMappingService actionMappingService;
        private readonly IConfigurationProvider configProvider;

        private readonly ConcurrentDictionary<string, ActionProcessor> eventProcessors = new ConcurrentDictionary<string, ActionProcessor>();
        private readonly ConcurrentQueue<ActionProcessor> closedProcessors = new ConcurrentQueue<ActionProcessor>();

        public ActionProcessorFactory(
            IActionService actionService,
            IActionMappingService actionMappingService,
            IConfigurationProvider configProvider)
        {
            this.actionService = actionService;
            this.actionMappingService = actionMappingService;
            this.configProvider = configProvider;
        }

        public IEventProcessor CreateEventProcessor(PartitionContext context)
        {
            var processor = new ActionProcessor(
                this.actionService,
                this.actionMappingService);

            processor.ProcessorClosed += this.ProcessorOnProcessorClosed;
            this.eventProcessors.TryAdd(context.Lease.PartitionId, processor);
            return processor;
        }

        public void ProcessorOnProcessorClosed(object sender, EventArgs eventArgs)
        {
            var processor = sender as ActionProcessor;
            if (processor != null)
            {
                this.eventProcessors.TryRemove(processor.Context.Lease.PartitionId, out processor);
                this.closedProcessors.Enqueue(processor);
            }
        }
    }
}
