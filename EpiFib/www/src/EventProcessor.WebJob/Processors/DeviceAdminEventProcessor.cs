// <copyright file="DeviceAdminEventProcessor.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace EventProcessor.WebJob.Processors
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using Common.Configuration;
    using Common.Helpers;
    using IoTInfrastructure.Services;
    using Microsoft.ServiceBus.Messaging;

    public class DeviceAdminEventProcessor : IDeviceAdminEventProcessor, IDisposable
    {
        private IConfigurationProvider configurationProvider;
        private IDeviceService deviceService;
        private CancellationTokenSource cancellationTokenSource;
        private EventProcessorHost eventProcessorHost;
        private DeviceAdminProcessorFactory factory;
        private bool isDisposed;
        private bool isRunning;

        public DeviceAdminEventProcessor(ILifetimeScope scope, IDeviceService deviceService)
        {
            EFGuard.NotNull(scope, nameof(scope));
            EFGuard.NotNull(deviceService, nameof(deviceService));

            this.configurationProvider = scope.Resolve<IConfigurationProvider>();
            this.deviceService = deviceService;
        }

        public void Start()
        {
            this.cancellationTokenSource = new CancellationTokenSource();
            this.Start(this.cancellationTokenSource.Token);
        }

        public void Start(CancellationToken cancellationToken)
        {
            this.isRunning = true;
            Task.Run(() => this.StartProcessor(cancellationToken), cancellationToken);
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
                this.cancellationTokenSource?.Dispose();
            }

            this.isDisposed = true;
        }

        private async Task StartProcessor(CancellationToken token)
        {
            try
            {
                // Initialize
                this.eventProcessorHost = new EventProcessorHost(
                    Environment.MachineName,
                    this.configurationProvider.GetConfigurationSettingValue("eventHub.MessageHubName").ToLowerInvariant(),
                    EventHubConsumerGroup.DefaultGroupName,
                    this.configurationProvider.GetConfigurationSettingValue("eventHub.ConnectionString"),
                    this.configurationProvider.GetConfigurationSettingValue("eventHub.StorageConnectionString"));

                this.factory = new DeviceAdminProcessorFactory(this.configurationProvider, this.deviceService);
                Trace.TraceInformation("DeviceEventProcessor: Registering host...");
                ////var options = new EventProcessorOptions();
                ////options.ExceptionReceived += OptionsOnExceptionReceived;
                await this.eventProcessorHost.RegisterEventProcessorFactoryAsync(this.factory);

                // processing loop
                while (!token.IsCancellationRequested)
                {
                    Trace.TraceInformation("DeviceEventProcessor: Processing...");
                    await Task.Delay(TimeSpan.FromMinutes(5), token);

                    // Any additional incremental processing can be done here (like checking states, etc).
                }

                // cleanup
                await this.eventProcessorHost.UnregisterEventProcessorAsync();
            }
            catch (Exception e)
            {
                Trace.TraceInformation("Error in DeviceEventProcessor.StartProcessor, Exception: {0}", e.Message);
            }

            this.isRunning = false;
        }
    }
}
