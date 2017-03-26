// <copyright file="IoTHubTransport.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace Simulator.WebJob.SimulatorCore.Transport
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using System.Threading.Tasks;
    using Common.Configuration;
    using Common.Helpers;
    using Microsoft.Azure.Devices.Client;
    using Newtonsoft.Json;
    using Simulator.WebJob.SimulatorCore.Devices;
    using Simulator.WebJob.SimulatorCore.Logging;

    public class IoTHubTransport : ITransport, IDisposable
    {
        private readonly ILogger logger;

        private readonly IConfigurationProvider configurationProvider;

        private readonly IDevice device;

        private DeviceClient deviceClient;

        private bool isDisposed = false;

        public IoTHubTransport(ILogger logger, IConfigurationProvider configurationProvider, IDevice device)
        {
            this.logger = logger;
            this.configurationProvider = configurationProvider;
            this.device = device;
        }

        ~IoTHubTransport()
        {
            this.Dispose(false);
        }

        public void Open()
        {
            if (string.IsNullOrWhiteSpace(this.device.DeviceID))
            {
                throw new ArgumentException("DeviceID value cannot be missing, null, or whitespace");
            }

            var str = this.GetConnectionString();
            this.deviceClient = DeviceClient.CreateFromConnectionString(str);
        }

        public async Task CloseAsync()
        {
            await this.deviceClient.CloseAsync();
        }

        public async Task SendEventAsync(dynamic eventData)
        {
            var eventId = Guid.NewGuid();
            await this.SendEventAsync(eventId, eventData);
        }

        public async Task SendEventAsync(Guid eventId, dynamic eventData)
        {
            string objectType = this.GetObjectType(eventData);
            var objectTypePrefix = this.configurationProvider.GetConfigurationSettingValue("simulator.ObjectTypePrefix");

            if (!string.IsNullOrWhiteSpace(objectType) && !string.IsNullOrEmpty(objectTypePrefix))
            {
                eventData.ObjectType = objectTypePrefix + objectType;
            }

            string rawJson = JsonConvert.SerializeObject(eventData);
            Trace.TraceInformation(rawJson);

            byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(eventData));

            var message = new Message(bytes);
            message.Properties["EventId"] = eventId.ToString();

            await AzureRetryHelper.OperationWithBasicRetryAsync(async () =>
            {
                try
                {
                    await deviceClient.SendEventAsync(message);
                }
                catch (Exception ex)
                {
                    logger.LogError(
                        "{0}{0}*** Exception: SendEventAsync ***{0}{0}EventId: {1}{0}Event Data: {2}{0}Exception: {3}{0}{0}",
                        Console.Out.NewLine,
                        eventId,
                        eventData,
                        ex);
                }
            });
        }

        public async Task SendEventBatchAsync(IEnumerable<Message> messages)
        {
            await this.deviceClient.SendEventBatchAsync(messages);
        }

        public async Task<DeserializableCommand> ReceiveAsync()
        {
            Message message = await AzureRetryHelper.OperationWithBasicRetryAsync(
                async () =>
                {
                    Exception exp = null;
                    Message msg = null;
                    try
                    {
                        msg = await deviceClient.ReceiveAsync();
                    }
                    catch (Exception exception)
                    {
                        exp = exception;
                    }

                    if (exp != null)
                    {
                        logger.LogError(
                            "{0}{0}*** Exception: ReceiveAsync ***{0}{0}{1}{0}{0}",
                            Console.Out.NewLine,
                            exp);

                        if (msg != null)
                        {
                            await deviceClient.AbandonAsync(msg);
                        }
                    }

                    return msg;
                });

            return message != null ? new DeserializableCommand(message) : null;
        }

        public async Task SignalAbandonedCommand(DeserializableCommand command)
        {
            EFGuard.NotNull(command, nameof(command));

            Debug.Assert(
                !string.IsNullOrEmpty(command.LockToken),
                "command.LockToken is a null reference or empty string.");

            await AzureRetryHelper.OperationWithBasicRetryAsync(
                async () =>
                {
                    try
                    {
                        await deviceClient.AbandonAsync(command.LockToken);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(
                            "{0}{0}*** Exception: Abandon Command ***{0}{0}Command Name: {1}{0}Command: {2}{0}Exception: {3}{0}{0}",
                            Console.Out.NewLine,
                            command.CommandName,
                            command.CommandHistory,
                            ex);
                    }
                });
        }

        public async Task SignalCompletedCommand(DeserializableCommand command)
        {
            EFGuard.NotNull(command, nameof(command));

            Debug.Assert(
                !string.IsNullOrEmpty(command.LockToken),
                "command.LockToken is a null reference or empty string.");

            await AzureRetryHelper.OperationWithBasicRetryAsync(
                async () =>
                {
                    try
                    {
                        await deviceClient.CompleteAsync(command.LockToken);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(
                            "{0}{0}*** Exception: Complete Command ***{0}{0}Command Name: {1}{0}Command: {2}{0}Exception: {3}{0}{0}",
                            Console.Out.NewLine,
                            command.CommandName,
                            command.CommandHistory,
                            ex);
                    }
                });
        }

        public async Task SignalRejectedCommand(DeserializableCommand command)
        {
            EFGuard.NotNull(command, nameof(command));

            Debug.Assert(
                !string.IsNullOrEmpty(command.LockToken),
                "command.LockToken is a null reference or empty string.");

            await AzureRetryHelper.OperationWithBasicRetryAsync(
                async () =>
                {
                    try
                    {
                        await deviceClient.RejectAsync(command.LockToken);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(
                            "{0}{0}*** Exception: Reject Command ***{0}{0}Command Name: {1}{0}Command: {2}{0}Exception: {3}{0}{0}",
                            Console.Out.NewLine,
                            command.CommandName,
                            command.CommandHistory,
                            ex);
                    }
                });
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
                this.deviceClient?.CloseAsync().Wait();
            }

            this.isDisposed = true;
        }

        private string GetConnectionString()
        {
            string key = this.device.PrimaryAuthKey;
            string deviceID = this.device.DeviceID;
            string hostName = this.device.HostName;

            var authMethod = new DeviceAuthenticationWithRegistrySymmetricKey(deviceID, key);
            return IotHubConnectionStringBuilder.Create(hostName + ".azure-devices.net", authMethod).ToString();
        }

        private string GetObjectType(dynamic eventData)
        {
            EFGuard.NotNull(eventData, nameof(eventData));

            var propertyInfo = eventData.GetType().GetProperty("ObjectType");
            if (propertyInfo == null)
            {
                return string.Empty;
            }

            var value = propertyInfo.GetValue(eventData, null);
            return value == null ? string.Empty : value.ToString();
        }
    }
}
