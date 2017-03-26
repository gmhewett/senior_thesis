// <copyright file="BulkDeviceTester.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace Simulator.WebJob
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Configuration;
    using Common.Models;
    using Common.Repository;
    using Simulator.WebJob.SimulatorCore.Devices;
    using Simulator.WebJob.SimulatorCore.Logging;
    using Simulator.WebJob.SimulatorCore.Telemetry;
    using Simulator.WebJob.SimulatorCore.Transport;

    public class BulkDeviceTester
    {
        private const int DefaultDevicePollIntervalSeconds = 120;

        private readonly ILogger logger;
        private readonly ITransportFactory transportFactory;
        private readonly IConfigurationProvider configProvider;
        private readonly ITelemetryFactory telemetryFactory;
        private readonly IDeviceFactory deviceFactory;
        private readonly IVirtualDeviceStorage deviceStorage;
        private readonly int devicePollIntervalSeconds;

        private List<InitialDeviceConfig> deviceList;
        
        public BulkDeviceTester(
            ITransportFactory transportFactory,
            ILogger logger,
            IConfigurationProvider configProvider,
            ITelemetryFactory telemetryFactory,
            IDeviceFactory deviceFactory,
            IVirtualDeviceStorage virtualDeviceStorage)
        {
            this.transportFactory = transportFactory;
            this.logger = logger;
            this.configProvider = configProvider;
            this.telemetryFactory = telemetryFactory;
            this.deviceFactory = deviceFactory;
            this.deviceStorage = virtualDeviceStorage;
            this.deviceList = new List<InitialDeviceConfig>();

            string pollingIntervalString = this.configProvider.GetConfigurationSettingValueOrDefault(
                                        "DevicePollIntervalSeconds",
                                        DefaultDevicePollIntervalSeconds.ToString(CultureInfo.InvariantCulture));

            this.devicePollIntervalSeconds = Convert.ToInt32(pollingIntervalString, CultureInfo.InvariantCulture);
        }

        public async Task ProcessDevicesAsync(CancellationToken token)
        {
            var dm = new DeviceManager(this.logger, token);

            try
            {
                this.logger.LogInfo("********** Starting Simulator **********");
                while (!token.IsCancellationRequested)
                {
                    var newDevices = new List<InitialDeviceConfig>();
                    var removedDevices = new List<string>();
                    var devices = await this.deviceStorage.GetDeviceListAsync();

                    if (devices != null && devices.Any())
                    {
                        newDevices = devices.Where(d => this.deviceList.All(x => x.DeviceId != d.DeviceId)).ToList();
                        if (this.deviceList != null)
                        {
                            removedDevices =
                                this.deviceList.Where(d => devices.All(x => x.DeviceId != d.DeviceId))
                                    .Select(x => x.DeviceId)
                                    .ToList();
                        }
                    }
                    else if (this.deviceList != null && this.deviceList.Any())
                    {
                        removedDevices = this.deviceList.Select(x => x.DeviceId).ToList();
                    }

                    if (newDevices.Count > 0)
                    {
                        this.logger.LogInfo("********** {0} NEW DEVICES FOUND ********** ", newDevices.Count);
                    }

                    if (removedDevices.Count > 0)
                    {
                        this.logger.LogInfo("********** {0} DEVICES REMOVED ********** ", removedDevices.Count);
                    }

                    this.deviceList = devices;

                    if (removedDevices.Any())
                    {
                        dm.StopDevices(removedDevices);
                    }

                    if (newDevices.Any())
                    {
                        var devicesToProcess = new List<IDevice>();

                        foreach (var deviceConfig in newDevices)
                        {
                            this.logger.LogInfo("********** SETTING UP NEW DEVICE : {0} ********** ", deviceConfig.DeviceId);
                            devicesToProcess.Add(
                                this.deviceFactory.CreateDevice(
                                    this.logger, 
                                    this.transportFactory, 
                                    this.telemetryFactory, 
                                    this.configProvider, 
                                    deviceConfig));
                        }

#pragma warning disable 4014
                        // don't wait for this to finish
                        dm.StartDevicesAsync(devicesToProcess);
#pragma warning restore 4014
                    }

                    await Task.Delay(TimeSpan.FromSeconds(this.devicePollIntervalSeconds), token);
                }
            }
            catch (TaskCanceledException)
            {
                this.logger.LogInfo("********** Primary worker role cancellation token source has been cancelled. **********");
            }
            finally
            {
                dm.StopAllDevices();
            }
        }
    }
}
