// <copyright file="Program.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace EventProcessor.WebJob
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using EventProcessor.WebJob.Processors;
    using Microsoft.Azure.WebJobs;
    using IContainer = Autofac.IContainer;

    public static class Program
    {
        private const string ShutdownFileEnvVar = "WEBJOBS_SHUTDOWN_FILE";

        private static readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
        private static string shutdownFile;
        private static IContainer eventProcessorContainer;

        private static void Main()
        {
            try
            {
                shutdownFile = Environment.GetEnvironmentVariable(ShutdownFileEnvVar);
                bool shutdownSignalReceived = false;

                if (!string.IsNullOrWhiteSpace(shutdownFile))
                {
                    string shutdownFilePath = Path.GetDirectoryName(shutdownFile);
                    if (!string.IsNullOrWhiteSpace(shutdownFilePath))
                    {
                        var fileSystemWatcher = new FileSystemWatcher(shutdownFilePath);
                        fileSystemWatcher.Created += OnShutdownFileChanged;
                        fileSystemWatcher.Changed += OnShutdownFileChanged;
                        fileSystemWatcher.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.FileName | NotifyFilters.LastWrite;
                        fileSystemWatcher.IncludeSubdirectories = false;
                        fileSystemWatcher.EnableRaisingEvents = true;

                        if (File.Exists(shutdownFile))
                        {
                            shutdownSignalReceived = true;
                        }
                    }
                }

                if (!shutdownSignalReceived)
                {
                    BuildContainer();

                    StartEventProcessorHost();
                    StartActionProcessorHost();
                    StartMessageFeedbackProcessorHost();

                    RunAsync().Wait();
                }

                var host = new JobHost();

                host.RunAndBlock();
            }
            catch (Exception ex)
            {
                CancellationTokenSource.Cancel();
                Trace.TraceError($"EventProcessor Webjob terminating: {ex}");
            }
        }

        private static void OnShutdownFileChanged(object sender, FileSystemEventArgs e)
        {
            string shutDownFilePath = Path.GetFileName(shutdownFile);

            if (!string.IsNullOrWhiteSpace(shutDownFilePath) && e.FullPath.IndexOf(shutDownFilePath, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                CancellationTokenSource.Cancel();
            }
        }

        private static void BuildContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new EventProcessorModule());
            eventProcessorContainer = builder.Build();
        }

        private static void StartEventProcessorHost()
        {
            Trace.TraceInformation("Starting Event Processor");
            var eventProcessor = eventProcessorContainer.Resolve<IDeviceAdminEventProcessor>();
            eventProcessor.Start();
        }

        private static void StartActionProcessorHost()
        {
            Trace.TraceInformation("Starting action processor");
            var actionProcessor = eventProcessorContainer.Resolve<IActionEventProcessor>();
            actionProcessor.Start();
        }

        private static void StartMessageFeedbackProcessorHost()
        {
            Trace.TraceInformation("Starting command feedback processor");
            var feedbackProcessor = eventProcessorContainer.Resolve<IMessageFeedbackProcessor>();
            feedbackProcessor.Start();
        }

        private static async Task RunAsync()
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
    }
}
