// <copyright file="MessageFeedbackProcessor.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace EventProcessor.WebJob.Processors
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Configuration;
    using Common.Helpers;
    using IoTInfrastructure.Services;
    using Microsoft.Azure.Devices;

    public class MessageFeedbackProcessor : IMessageFeedbackProcessor, IDisposable
    {
        private readonly IDeviceService deviceService;
        private readonly string ioTHubConnectionString;
        private CancellationTokenSource cancellationTokenSource;
        private bool isRunning;
        private bool isDisposed;

        public MessageFeedbackProcessor(IConfigurationProvider configProvider, IDeviceService deviceService)
        {
            EFGuard.NotNull(configProvider, nameof(configProvider));
            EFGuard.NotNull(deviceService, nameof(deviceService));

            this.ioTHubConnectionString = configProvider.GetConfigurationSettingValue("iotHub.ConnectionString");

            if (string.IsNullOrEmpty(this.ioTHubConnectionString))
            {
                throw new InvalidOperationException("Cannot find configuration setting: \"iotHub.ConnectionString\".");
            }

            this.deviceService = deviceService;
        }

        ~MessageFeedbackProcessor()
        {
            this.Dispose(false);
        }

        public void Start()
        {
            this.isRunning = true;
            this.cancellationTokenSource = new CancellationTokenSource();

            Task.Run(
                () => this.RunProcess(this.cancellationTokenSource.Token),
                this.cancellationTokenSource.Token);
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

        private async Task RunProcess(CancellationToken token)
        {
            EFGuard.NotNull(token, nameof(token));

            ServiceClient serviceClient = null;
            try
            {
                serviceClient = ServiceClient.CreateFromConnectionString(this.ioTHubConnectionString);
                await serviceClient.OpenAsync();

                while (!token.IsCancellationRequested)
                {
                    var batchReceiver = serviceClient.GetFeedbackReceiver();
                    var batch = await batchReceiver.ReceiveAsync(TimeSpan.FromSeconds(10.0));

                    IEnumerable<FeedbackRecord> records;
                    if ((batch == null) || ((records = batch.Records) == null))
                    {
                        continue;
                    }

                    records = records.Where(t => t != null)
                        .Where(x => !string.IsNullOrEmpty(x.DeviceId))
                        .Where(x => !string.IsNullOrEmpty(x.OriginalMessageId));

                    foreach (FeedbackRecord record in records)
                    {
                        this.UpdateDeviceRecord(record, batch.EnqueuedTime);
                    }

                    await batchReceiver.CompleteAsync(batch);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Error in MessageFeedbackProcessor.RunProcess, Exception: {ex.Message}");
            }
            finally
            {
                if (serviceClient != null)
                {
                    await serviceClient.CloseAsync();
                }

                this.isRunning = false;
            }
        }

        private async void UpdateDeviceRecord(FeedbackRecord record, DateTime enqueuDateTime)
        {
            Trace.TraceInformation(
                            "{0}{0}*** Processing Feedback Record ***{0}{0}DeviceId: {1}{0}OriginalMessageId: {2}{0}Result: {3}{0}{0}",
                            Console.Out.NewLine,
                            record.DeviceId,
                            record.OriginalMessageId,
                            record.StatusCode);

            var device = await this.deviceService.GetDeviceAsync(record.DeviceId);
            var existingCommand = device?.CommandHistory.FirstOrDefault(x => x.MessageId == record.OriginalMessageId);
            if (existingCommand == null)
            {
                return;
            }

            var updatedTime = record.EnqueuedTimeUtc;
            if (updatedTime == default(DateTime))
            {
                updatedTime = enqueuDateTime == default(DateTime) ? DateTime.UtcNow : enqueuDateTime;
            }

            existingCommand.UpdatedTime = updatedTime;
            existingCommand.Result = record.StatusCode.ToString();

            if (record.StatusCode == FeedbackStatusCode.Success)
            {
                existingCommand.ErrorMessage = string.Empty;
            }
            
            await this.deviceService.UpdateDeviceAsync(device);
        }
    }
}