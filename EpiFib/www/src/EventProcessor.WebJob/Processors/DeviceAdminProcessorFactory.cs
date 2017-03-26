// <copyright file="DeviceAdminProcessorFactory.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace EventProcessor.WebJob.Processors
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.Configuration;
    using Common.Helpers;
    using IoTInfrastructure.Services;
    using Microsoft.ServiceBus.Messaging;

    public class DeviceAdminProcessorFactory : IEventProcessorFactory
    {
        private readonly IConfigurationProvider configProvider;
        private readonly IDeviceService deviceService;

        private readonly ConcurrentDictionary<string, DeviceAdminProcessor> eventProcessors = new ConcurrentDictionary<string, DeviceAdminProcessor>();
        private readonly ConcurrentQueue<DeviceAdminProcessor> closedProcessors = new ConcurrentQueue<DeviceAdminProcessor>();

        public DeviceAdminProcessorFactory(IConfigurationProvider configProvider, IDeviceService deviceService)
        {
            EFGuard.NotNull(configProvider, nameof(configProvider));
            EFGuard.NotNull(deviceService, nameof(deviceService));

            this.configProvider = configProvider;
            this.deviceService = deviceService;
        }

        public int ActiveProcessors => this.eventProcessors.Count;

        public int TotalMessages
        {
            get
            {
                int amount = this.eventProcessors.Select(p => p.Value.TotalMessages).Sum();
                amount += this.closedProcessors.Select(p => p.TotalMessages).Sum();
                return amount;
            }
        }

        public IEventProcessor CreateEventProcessor(PartitionContext context)
        {
            var processor = new DeviceAdminProcessor(this.configProvider, this.deviceService);
            processor.ProcessorClosed += this.ProcessorOnProcessorClosed;
            this.eventProcessors.TryAdd(context.Lease.PartitionId, processor);
            return processor;
        }

        public Task WaitForAllProcessorsInitialized(TimeSpan timeout)
        {
            return this.WaitForAllProcessorsCondition(p => p.IsInitialized, timeout);
        }

        public Task WaitForAllProcessorsClosed(TimeSpan timeout)
        {
            return this.WaitForAllProcessorsCondition(p => p.IsClosed, timeout);
        }

        public async Task WaitForAllProcessorsCondition(Func<DeviceAdminProcessor, bool> predicate, TimeSpan timeout)
        {
            TimeSpan sleepInterval = TimeSpan.FromSeconds(2);
            while (!this.eventProcessors.Values.All(predicate))
            {
                if (timeout > sleepInterval)
                {
                    timeout = timeout.Subtract(sleepInterval);
                }
                else
                {
                    throw new TimeoutException("Condition not satisfied within expected timeout.");
                }

                await Task.Delay(sleepInterval);
            }
        }

        public void ProcessorOnProcessorClosed(object sender, EventArgs eventArgs)
        {
            var processor = sender as DeviceAdminProcessor;
            if (processor != null)
            {
                this.eventProcessors.TryRemove(processor.Context.Lease.PartitionId, out processor);
                this.closedProcessors.Enqueue(processor);
            }
        }
    }
}
