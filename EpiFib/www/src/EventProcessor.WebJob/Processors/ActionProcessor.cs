// <copyright file="ActionProcessor.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace EventProcessor.WebJob.Processors
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using System.Threading.Tasks;
    using IoTInfrastructure.Models;
    using IoTInfrastructure.Services;
    using Microsoft.ServiceBus.Messaging;
    using Newtonsoft.Json;

    public class ActionProcessor : IEventProcessor
    {
        private readonly IActionService actionService;
        private readonly IActionMappingService actionMappingService;

        private Stopwatch checkpointStopwatch;

        public ActionProcessor(
            IActionService actionService,
            IActionMappingService actionMappingService)
        {
            this.LastMessageOffset = "-1";
            this.actionService = actionService;
            this.actionMappingService = actionMappingService;
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
            Trace.TraceInformation("ActionProcessor: Open : Partition : {0}", context.Lease.PartitionId);
            this.Context = context;
            this.checkpointStopwatch = new Stopwatch();
            this.checkpointStopwatch.Start();

            this.IsInitialized = true;

            return Task.Delay(0);
        }

        public async Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            Trace.TraceInformation("ActionProcessor: In ProcessEventsAsync");

            foreach (EventData message in messages)
            {
                try
                {
                    Trace.TraceInformation("ActionProcessor: {0} - Partition {1}", message.Offset, context.Lease.PartitionId);
                    this.LastMessageOffset = message.Offset;

                    string jsonString = Encoding.UTF8.GetString(message.GetBytes());
                    IList<ActionModel> results = JsonConvert.DeserializeObject<List<ActionModel>>(jsonString);
                    if (results != null)
                    {
                        foreach (ActionModel item in results)
                        {
                            await this.ProcessAction(item);
                        }
                    }

                    ++this.TotalMessages;
                }
                catch (Exception e)
                {
                    Trace.TraceError($"ActionProcessor: Error in ProcessEventAsync -- {e}");
                }
            }

            // checkpoint after processing batch
            try
            {
                await context.CheckpointAsync();
            }
            catch (Exception ex)
            {
                Trace.TraceError(
                    "{0}{0}*** CheckpointAsync Exception - ActionProcessor.ProcessEventsAsync ***{0}{0}{1}{0}{0}",
                    Console.Out.NewLine,
                    ex);
            }

            if (this.IsClosed)
            {
                this.IsReceivedMessageAfterClose = true;
            }
        }

        public Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            Trace.TraceInformation("ActionProcessor: Close : Partition : " + context.Lease.PartitionId);
            this.IsClosed = true;
            this.checkpointStopwatch.Stop();
            this.CloseReason = reason;
            this.OnProcessorClosed();

            try
            {
                return context.CheckpointAsync();
            }
            catch (Exception ex)
            {
                Trace.TraceError(
                    "{0}{0}*** CheckpointAsync Exception - ActionProcessor.CloseAsync ***{0}{0}{1}{0}{0}",
                    Console.Out.NewLine,
                    ex);

                return Task.Run(() => { });
            }
        }

        public virtual void OnProcessorClosed()
        {
            this.ProcessorClosed?.Invoke(this, EventArgs.Empty);
        }

        private async Task ProcessAction(ActionModel eventData)
        {
            if (eventData == null)
            {
                Trace.TraceWarning("Action event is null");
                return;
            }

            try
            {
                // NOTE: all column names from ASA come out as lowercase; see 
                // https://social.msdn.microsoft.com/Forums/office/en-US/c79a662b-5db1-4775-ba1a-23df1310091d/azure-table-storage-account-output-property-names-are-lowercase?forum=AzureStreamAnalytics 
                string deviceId = eventData.DeviceID;
                string ruleOutput = eventData.RuleOutput;

                if (ruleOutput.Equals("AlarmTemp", StringComparison.OrdinalIgnoreCase))
                {
                    Trace.TraceInformation("ProcessAction: temperature rule triggered!");
                    double tempReading = eventData.Reading;

                    string tempActionId = await this.actionMappingService.GetActionIdFromRuleOutputAsync(ruleOutput);

                    if (!string.IsNullOrWhiteSpace(tempActionId))
                    {
                        await this.actionService.ExecuteLogicAppAsync(
                        tempActionId,
                        deviceId,
                        "Temperature",
                        tempReading);
                    }
                    else
                    {
                        Trace.TraceError("ActionProcessor: tempActionId value is empty for temperatureRuleOutput '{0}'", ruleOutput);
                    }
                }

                if (ruleOutput.Equals("AlarmHumidity", StringComparison.OrdinalIgnoreCase))
                {
                    Trace.TraceInformation("ProcessAction: humidity rule triggered!");
                    double humidityReading = eventData.Reading;

                    string humidityActionId = await this.actionMappingService.GetActionIdFromRuleOutputAsync(ruleOutput);

                    if (!string.IsNullOrWhiteSpace(humidityActionId))
                    {
                        await this.actionService.ExecuteLogicAppAsync(
                            humidityActionId,
                            deviceId,
                            "Humidity",
                            humidityReading);
                    }
                    else
                    {
                        Trace.TraceError("ActionProcessor: humidityActionId value is empty for humidityRuleOutput '{0}'", ruleOutput);
                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("ActionProcessor: exception in ProcessAction:");
                Trace.TraceError(e.ToString());
            }
        }
    }
}
