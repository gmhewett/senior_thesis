// <copyright file="DeviceBase.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace Simulator.WebJob.SimulatorCore.Devices
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Configuration;
    using Common.Factory;
    using Common.Helpers;
    using Common.Models;
    using Microsoft.Azure.Devices.Client.Exceptions;
    using Simulator.WebJob.SimulatorCore.CommandProcessors;
    using Simulator.WebJob.SimulatorCore.Logging;
    using Simulator.WebJob.SimulatorCore.Telemetry;
    using Simulator.WebJob.SimulatorCore.Transport;

    public class DeviceBase : IDevice
    {
        protected readonly ILogger Logger;

        protected readonly ITransportFactory TransportFactory;

        protected readonly ITelemetryFactory TelemetryFactory;

        protected readonly IConfigurationProvider ConfigProvider;
        
        private int currentEventGroup;

        public DeviceBase(
            ILogger logger,
            ITransportFactory transportFactory,
            ITelemetryFactory telemetryFactory,
            IConfigurationProvider configurationProvider)
        {
            this.ConfigProvider = configurationProvider;
            this.Logger = logger;
            this.TransportFactory = transportFactory;
            this.TelemetryFactory = telemetryFactory;
            this.TelemetryEvents = new List<ITelemetry>();
        }

        public string DeviceID { get; set; }

        public string HostName { get; set; }

        public string PrimaryAuthKey { get; set; }

        public DeviceProperties DeviceProperties { get; set; }

        public List<Command> Commands { get; set; }

        public List<Telemetry> Telemetry { get; set; }

        public List<ITelemetry> TelemetryEvents { get; set; }

        public bool RepeatEventListForever { get; set; }

        protected ITransport Transport { get; set; }

        protected CommandProcessor RootCommandProcessor { get; set; }

        protected object TelemetryController { get; set; }

        public void Init(InitialDeviceConfig config)
        {
            this.InitDeviceInfo(config);

            this.Transport = this.TransportFactory.CreateTransport(this);
            this.TelemetryController = this.TelemetryFactory.PopulateDeviceWithTelemetryEvents(this);

            this.InitCommandProcessors();
        }

        public async Task SendDeviceInfo()
        {
            this.Logger.LogInfo($"Sending Device Info for device {DeviceID}...");
            await this.Transport.SendEventAsync(this.GetDeviceInfo());
        }

        public DeviceModel GetDeviceInfo()
        {
            DeviceModel device = DeviceCreatorHelper.BuildDeviceStructure(this.DeviceID, true, null);
            device.DeviceProperties = this.DeviceProperties;
            device.Commands = this.Commands ?? new List<Command>();
            device.Telemetry = this.Telemetry ?? new List<Telemetry>();
            device.Version = SampleDeviceFactory.Version_1_0;
            device.ObjectType = SampleDeviceFactory.ObjectTypeDeviceInfo;
            device.SystemProperties = null;

            return device;
        }

        public async Task StartAsync(CancellationToken token)
        {
            try
            {
                this.Transport.Open();

                var loopTasks = new List<Task>
                {
                    this.StartReceiveLoopAsync(token),
                    this.StartSendLoopAsync(token)
                };

                // Wait both the send and receive loops
                await Task.WhenAll(loopTasks.ToArray());

                // once the code makes it here the token has been canceled
                await this.Transport.CloseAsync();
            }
            catch (Exception ex)
            {
                this.Logger.LogError("Unexpected Exception starting device: {0}", ex.ToString());
            }
        }

        protected virtual void InitDeviceInfo(InitialDeviceConfig config)
        {
            DeviceModel initialDevice = SampleDeviceFactory.GetSampleSimulatedDevice(config.DeviceId, config.Key);
            DeviceProperties = initialDevice.DeviceProperties;
            this.Commands = initialDevice.Commands ?? new List<Command>();
            this.Telemetry = initialDevice.Telemetry ?? new List<Telemetry>();
            this.DeviceID = config.DeviceId;
            this.HostName = config.HostName;
            this.PrimaryAuthKey = config.Key;
        }

        protected virtual void InitCommandProcessors()
        {
            var pingDeviceProcessor = new PingDeviceProcessor(this);

            this.RootCommandProcessor = pingDeviceProcessor;
        }

        private async Task StartReceiveLoopAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    DeserializableCommand command = null;
                    Exception exception = null;

                    // Pause before running through the receive loop
                    await Task.Delay(TimeSpan.FromSeconds(10), token);
                    this.Logger.LogInfo("Device {0} checking for commands...", this.DeviceID);

                    try
                    {
                        // Retrieve the message from the IoT Hub
                        command = await this.Transport.ReceiveAsync();

                        if (command == null)
                        {
                            continue;
                        }

                        var processingResult = await this.RootCommandProcessor.HandleCommandAsync(command);

                        switch (processingResult)
                        {
                            case CommandProcessingResult.CannotComplete:
                                await this.Transport.SignalRejectedCommand(command);
                                break;

                            case CommandProcessingResult.RetryLater:
                                await this.Transport.SignalAbandonedCommand(command);
                                break;

                            case CommandProcessingResult.Success:
                                await this.Transport.SignalCompletedCommand(command);
                                break;
                        }

                        this.Logger.LogInfo(
                            "Device: {1}{0}Command: {2}{0}Lock token: {3}{0}Result: {4}{0}",
                            Console.Out.NewLine,
                            this.DeviceID,
                            command.CommandName,
                            command.LockToken,
                            processingResult);
                    }
                    catch (IotHubException ex)
                    {
                        exception = ex;

                        this.Logger.LogInfo(
                            "Device: {1}{0}Command: {2}{0}Lock token: {3}{0}Error Type: {4}{0}Exception: {5}{0}",
                            Console.Out.NewLine,
                            this.DeviceID,
                            command?.CommandName ?? string.Empty,
                            command?.LockToken ?? string.Empty,
                            ex.IsTransient ? "Transient Error" : "Non-transient Error",
                            ex.ToString());
                    }
                    catch (Exception ex)
                    {
                        exception = ex;

                        this.Logger.LogInfo(
                            "Device: {1}{0}Command: {2}{0}Lock token: {3}{0}Exception: {4}{0}",
                            Console.Out.NewLine,
                            this.DeviceID,
                            command?.CommandName ?? string.Empty,
                            command?.LockToken ?? string.Empty,
                            ex.ToString());
                    }

                    if ((command != null) &&
                        (exception != null))
                    {
                        await this.Transport.SignalAbandonedCommand(command);
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // do nothing if the task was cancelled
            }
            catch (Exception ex)
            {
                this.Logger.LogError("Unexpected Exception starting device receive loop: {0}", ex.ToString());
            }

            this.Logger.LogInfo(
                "********** Processing Device {0} has been cancelled - StartReceiveLoopAsync Ending. **********", 
                this.DeviceID);
        }

        private async Task StartSendLoopAsync(CancellationToken token)
        {
            try
            {
                this.Logger.LogInfo("Booting device {0}...", this.DeviceID);

                do
                {
                    this.currentEventGroup = 0;

                    this.Logger.LogInfo("Starting events list for device {0}...", this.DeviceID);

                    while (this.currentEventGroup < this.TelemetryEvents.Count && !token.IsCancellationRequested)
                    {
                        this.Logger.LogInfo("Device {0} starting IEventGroup {1}...", this.DeviceID, this.currentEventGroup);

                        var eventGroup = this.TelemetryEvents[this.currentEventGroup];

                        await eventGroup.SendEventsAsync(
                            token, 
                            async eventData =>
                            {
                                await this.Transport.SendEventAsync(eventData);
                            });

                        this.currentEventGroup++;
                    }

                    this.Logger.LogInfo("Device {0} finished sending all events in list...", this.DeviceID);
                }
                while (this.RepeatEventListForever && !token.IsCancellationRequested);

                this.Logger.LogWarning(
                    "Device {0} sent all events and is shutting down send loop. (Set RepeatEventListForever = true on the device to loop forever.)",
                    this.DeviceID);
            }
            catch (TaskCanceledException)
            {
                // do nothing if the task was cancelled
            }
            catch (Exception ex)
            {
                this.Logger.LogError("Unexpected Exception starting device send loop: {0}", ex.ToString());
            }

            if (token.IsCancellationRequested)
            {
               this.Logger.LogInfo(
                   "********** Processing Device {0} has been cancelled - StartSendLoopAsync Ending. **********",
                   this.DeviceID);
            }
        }
    }
}
