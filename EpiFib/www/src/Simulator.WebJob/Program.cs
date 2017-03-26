// <copyright file="Program.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace Simulator.WebJob
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using Common.Configuration;
    using Common.Helpers;
    using Common.Repository;
    using Simulator.WebJob.Container.Devices;
    using Simulator.WebJob.Container.Telemetry;
    using Simulator.WebJob.DataInitialization;
    using Simulator.WebJob.SimulatorCore;
    using Simulator.WebJob.SimulatorCore.Devices;
    using Simulator.WebJob.SimulatorCore.Logging;
    using Simulator.WebJob.SimulatorCore.Transport;

    public static class Program
    {
        private const string ShutdownFileEnvVar = "WEBJOBS_SHUTDOWN_FILE";

        private static readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
        private static string shutdownFile;
        private static IContainer simulatorContainer;
        private static Timer timer;

        public static void Main(string[] args)
        {
            try
            {
                // Cloud deploys often get staged and started to warm them up, then get a shutdown
                // signal from the framework before being moved to the production slot. We don't want 
                // to start initializing data if we have already gotten the shutdown message, so we'll 
                // monitor it. This environment variable is reliable
                // http://blog.amitapple.com/post/2014/05/webjobs-graceful-shutdown/#.VhVYO6L8-B4
                shutdownFile = Environment.GetEnvironmentVariable(ShutdownFileEnvVar);
                bool shutdownSignalReceived = false;

                // Setup a file system watcher on that file's directory to know when the file is created
                // First check for null, though. This does not exist on a localhost deploy, only cloud
                if (!string.IsNullOrWhiteSpace(shutdownFile))
                {
                    var fileSystemWatcher = new FileSystemWatcher(Path.GetDirectoryName(shutdownFile));
                    fileSystemWatcher.Created += OnShutdownFileChanged;
                    fileSystemWatcher.Changed += OnShutdownFileChanged;
                    fileSystemWatcher.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.FileName | NotifyFilters.LastWrite;
                    fileSystemWatcher.IncludeSubdirectories = false;
                    fileSystemWatcher.EnableRaisingEvents = true;

                    // In case the file had already been created before we started watching it.
                    if (File.Exists(shutdownFile))
                    {
                        shutdownSignalReceived = true;
                    }
                }

                if (!shutdownSignalReceived)
                {
                    BuildContainer();

                    StartDataInitializationAsNeeded();
                    StartSimulator();

                    RunAsync().Wait();
                }
            }
            catch (Exception ex)
            {
                CancellationTokenSource.Cancel();
                Trace.TraceError($"Webjob terminating: {ex}");
            }
        }

        public static void BuildContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new SimulatorModule());
            simulatorContainer = builder.Build();
        }

        public static void CreateInitialDataAsNeeded(object state)
        {
            timer.Dispose();
            if (!CancellationTokenSource.Token.IsCancellationRequested)
            {
                Trace.TraceInformation("Preparing to add initial data");
                var creator = simulatorContainer.Resolve<IDataInitializer>();
                creator.CreateInitialDataIfNeeded();
            }
        }

        public static void StartDataInitializationAsNeeded()
        {
            timer = new Timer(CreateInitialDataAsNeeded, null, 10000, Timeout.Infinite);
        }

        public static void StartSimulator()
        {
            // Dependencies to inject into the Bulk DeviceBase Tester
            var logger = new TraceLogger();
            var configProvider = new ConfigurationProvider();
            var tableStorageClientFactory = new AzureTableStorageClientFactory();
            var telemetryFactory = new ContainerTelemetryFactory(logger);

            var transportFactory = new IoTHubTransportFactory(logger, configProvider);

            IVirtualDeviceStorage deviceStorage = null;
            var useConfigforDeviceList = Convert.ToBoolean(configProvider.GetConfigurationSettingValueOrDefault("UseConfigForDeviceList", "False"), CultureInfo.InvariantCulture);

            if (useConfigforDeviceList)
            {
                deviceStorage = new AppConfigRepository(configProvider, logger);
            }
            else
            {
                deviceStorage = new VirtualDeviceTableStorage(configProvider, tableStorageClientFactory);
            }

            IDeviceFactory deviceFactory = new ContainerDeviceFactory();

            // Start Simulator
            Trace.TraceInformation("Starting Simulator");
            var tester = new BulkDeviceTester(transportFactory, logger, configProvider, telemetryFactory, deviceFactory, deviceStorage);
            Task.Run(() => tester.ProcessDevicesAsync(CancellationTokenSource.Token), CancellationTokenSource.Token);
        }

        public static async Task RunAsync()
        {
            while (!CancellationTokenSource.Token.IsCancellationRequested)
            {
                Trace.TraceInformation("Running");
                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(5), CancellationTokenSource.Token);
                }
                catch (TaskCanceledException)
                {
                }
            }
        }

        private static void OnShutdownFileChanged(object sender, FileSystemEventArgs e)
        {
            if (e.FullPath.IndexOf(Path.GetFileName(shutdownFile), StringComparison.OrdinalIgnoreCase) >= 0)
            {
                CancellationTokenSource.Cancel();
            }
        }
    }
}
