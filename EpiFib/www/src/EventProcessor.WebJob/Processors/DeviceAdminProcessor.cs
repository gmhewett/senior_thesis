// <copyright file="DeviceAdminProcessor.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace EventProcessor.WebJob.Processors
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using System.Threading.Tasks;
    using Common.Configuration;
    using Common.Exceptions;
    using Common.Factory;
    using Common.Helpers;
    using Common.Models;
    using IoTInfrastructure.Services;
    using Microsoft.ServiceBus.Messaging;
    using Newtonsoft.Json;

    public class DeviceAdminProcessor : IEventProcessor
    {
        private readonly IConfigurationProvider configProvider;
        private readonly IDeviceService deviceService;

        private Stopwatch checkpointStopWatch;

        public DeviceAdminProcessor(IConfigurationProvider configProvider, IDeviceService deviceService)
        {
            EFGuard.NotNull(configProvider, nameof(configProvider));
            EFGuard.NotNull(deviceService, nameof(deviceService));

            this.configProvider = configProvider;
            this.deviceService = deviceService;
        }

        public event EventHandler ProcessorClosed;

        public bool IsInitialized { get; private set; }

        public bool IsClosed { get; private set; }

        public bool IsReceivedMessageAfterClose { get; set; }

        public int TotalMessages { get; set; }

        public CloseReason CloseReason { get; private set; }

        public PartitionContext Context { get; private set; }

        public string LastMessageOffset { get; private set; }

        public Task OpenAsync(PartitionContext context)
        {
            Trace.TraceInformation($"DeviceAdminProcessor: Open : Partition : {context.Lease.PartitionId}");

            this.Context = context;
            this.checkpointStopWatch = new Stopwatch();
            this.checkpointStopWatch.Start();
            this.IsInitialized = true;

            return Task.Delay(0);
        }

        public Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            Trace.TraceInformation($"DeviceAdministrationProcessor: Close : Partition : {context.Lease.PartitionId}");
            this.IsClosed = true;
            this.checkpointStopWatch.Stop();
            this.CloseReason = reason;
            this.OnProcessorClosed();

            try
            {
                return context.CheckpointAsync();
            }
            catch (Exception ex)
            {
                Trace.TraceError(
                    "{0}{0}*** CheckpointAsync Exception - DeviceAdministrationProcessor.CloseAsync ***{0}{0}{1}{0}{0}",
                    Console.Out.NewLine,
                    ex);

                return Task.Run(() => { });
            }
        }

        public async Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            Trace.TraceInformation("DeviceAdministrationProcessor: In ProcessEventsAsync");

            foreach (EventData message in messages)
            {
                try
                {
                    // Write out message
                    Trace.TraceInformation("DeviceAdministrationProcessor: {0} - Partition {1}", message.Offset, context.Lease.PartitionId);
                    this.LastMessageOffset = message.Offset;

                    string jsonString = Encoding.UTF8.GetString(message.GetBytes());
                    IList<DeviceModel> results = JsonConvert.DeserializeObject<List<DeviceModel>>(jsonString);
                    if (results != null)
                    {
                        foreach (DeviceModel resultItem in results)
                        {
                            await this.ProcessEventItem(resultItem);
                        }
                    }

                    this.TotalMessages++;
                }
                catch (Exception e)
                {
                    Trace.TraceInformation("DeviceAdministrationProcessor: Error in ProcessEventAsync -- " + e.Message);
                }
            }

            // batch has been processed, checkpoint 
            try
            {
                await context.CheckpointAsync();
            }
            catch (Exception ex)
            {
                Trace.TraceError(
                    "{0}{0}*** CheckpointAsync Exception - DeviceAdministrationProcessor.ProcessEventsAsync ***{0}{0}{1}{0}{0}",
                    Console.Out.NewLine,
                    ex);
            }

            if (this.IsClosed)
            {
                this.IsReceivedMessageAfterClose = true;
            }

            await Task.Delay(0);
        }

        public virtual void OnProcessorClosed()
        {
            this.ProcessorClosed?.Invoke(this, EventArgs.Empty);
        }

        private async Task ProcessEventItem(DeviceModel eventData)
        {
            if (eventData?.ObjectType == null)
            {
                Trace.TraceWarning("Event has no ObjectType defined.  No action was taken on Event packet.");
                return;
            }

            string objectType = eventData.ObjectType;

            var objectTypePrefix = this.configProvider.GetConfigurationSettingValue("ObjectTypePrefix");
            if (string.IsNullOrWhiteSpace(objectTypePrefix))
            {
                objectTypePrefix = string.Empty;
            }

            if (objectType == objectTypePrefix + SampleDeviceFactory.ObjectTypeDeviceInfo)
            {
                await this.ProcessDeviceInfo(eventData);
            }
            else
            {
                Trace.TraceWarning("Unknown ObjectType in event.");
            }
        }

        private async Task ProcessDeviceInfo(DeviceModel deviceInfo)
        {
            string versionAsString = string.Empty;
            if (deviceInfo.Version != null)
            {
                versionAsString = deviceInfo.Version;
            }

            switch (versionAsString)
            {
                case SampleDeviceFactory.Version_1_0:

                    // Data coming in from the simulator can sometimes turn a boolean into 0 or 1.
                    // Check the HubEnabledState since this is actually displayed and make sure it's in a good format
                    // Should not be required for strongly typed object
                    // DeviceSchemaHelperND.FixDeviceSchema(deviceInfo);
                    if (deviceInfo.IoTHub == null)
                    {
                        throw new DeviceRequiredPropertyNotFoundException("'IoTHub' property is missing");
                    }

                    string name = deviceInfo.IoTHub.ConnectionDeviceId;
                    Trace.TraceInformation($"ProcessEventAsync -- DeviceInfo: {name}");
                    await this.deviceService.UpdateDeviceFromDeviceInfoPacketAsync(deviceInfo);

                    break;
                default:
                    Trace.TraceInformation($"Unknown version {versionAsString} provided in Device Info packet");
                    break;
            }
        }
    }
}
