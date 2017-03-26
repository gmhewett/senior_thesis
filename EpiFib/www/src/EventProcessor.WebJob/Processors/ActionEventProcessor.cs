// <copyright file="ActionEventProcessor.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace EventProcessor.WebJob.Processors
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using Common.Configuration;
    using IoTInfrastructure.Services;
    using Microsoft.ServiceBus.Messaging;

    public class ActionEventProcessor : IActionEventProcessor, IDisposable
    {
        private readonly IActionService actionLogic;
        private readonly IActionMappingService actionMappingLogic;
        private readonly IConfigurationProvider configurationProvider;

        private EventProcessorHost eventProcessorHost;
        private ActionProcessorFactory factory;
        private CancellationTokenSource cancellationTokenSource;
        private bool isRunning = false;
        private bool isDisposed = false;

        public ActionEventProcessor(
            ILifetimeScope lifetimeScope,
            IActionService actionLogic,
            IActionMappingService actionMappingLogic)
        {
            this.configurationProvider = lifetimeScope.Resolve<IConfigurationProvider>();
            this.actionLogic = actionLogic;
            this.actionMappingLogic = actionMappingLogic;
        }

        ~ActionEventProcessor()
        {
            this.Dispose(false);
        }

        public void Start()
        {
            this.isRunning = true;
            this.cancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => this.StartProcessor(this.cancellationTokenSource.Token), this.cancellationTokenSource.Token);
        }

        public void Stop()
        {
            this.cancellationTokenSource.Cancel();
            TimeSpan timeout = TimeSpan.FromSeconds(30);
            TimeSpan sleepInterval = TimeSpan.FromSeconds(1);
            while (this.isRunning)
            {
                if (timeout < sleepInterval)
                {
                    break;
                }

                Thread.Sleep(sleepInterval);
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                if (this.cancellationTokenSource != null)
                {
                    this.cancellationTokenSource.Dispose();
                }
            }

            this.isDisposed = true;
        }

        private async Task StartProcessor(CancellationToken token)
        {
            try
            {
                string hostName = Environment.MachineName;
                string eventHubPath = this.configurationProvider.GetConfigurationSettingValue("eventHub.AlarmHubName").ToLowerInvariant();
                string consumerGroup = EventHubConsumerGroup.DefaultGroupName;
                string eventHubConnectionString = this.configurationProvider.GetConfigurationSettingValue("eventHub.ConnectionString");
                string storageConnectionString = this.configurationProvider.GetConfigurationSettingValue("device.StorageConnectionString");

                this.eventProcessorHost = new EventProcessorHost(
                    hostName,
                    eventHubPath.ToLower(),
                    consumerGroup,
                    eventHubConnectionString,
                    storageConnectionString);

                this.factory = new ActionProcessorFactory(
                    this.actionLogic,
                    this.actionMappingLogic,
                    this.configurationProvider);

                Trace.TraceInformation("ActionEventProcessor: Registering host...");
                var options = new EventProcessorOptions();
                options.ExceptionReceived += this.OptionsOnExceptionReceived;
                await this.eventProcessorHost.RegisterEventProcessorFactoryAsync(this.factory);

                // processing loop
                while (!token.IsCancellationRequested)
                {
                    Trace.TraceInformation("ActionEventProcessor: Processing...");
                    await Task.Delay(TimeSpan.FromMinutes(5), token);
                }

                // cleanup
                await this.eventProcessorHost.UnregisterEventProcessorAsync();
            }
            catch (Exception e)
            {
                Trace.TraceError("Error in ActionProcessor.StartProcessor, Exception: {0}", e.ToString());
            }

            this.isDisposed = false;
        }

        private void OptionsOnExceptionReceived(object sender, ExceptionReceivedEventArgs args)
        {
            Trace.TraceError($"Received exception, action: {args.Action}, exception: {args.Exception}");
        }
    }
}
