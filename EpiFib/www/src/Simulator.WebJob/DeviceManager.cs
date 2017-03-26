// <copyright file="DeviceManager.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace Simulator.WebJob
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Simulator.WebJob.SimulatorCore.Devices;
    using Simulator.WebJob.SimulatorCore.Logging;

    public class DeviceManager
    {
        private readonly ILogger logger;
        private readonly CancellationToken token;
        private readonly Dictionary<string, CancellationTokenSource> cancellationTokens;
        
        public DeviceManager(ILogger logger, CancellationToken token)
        {
            this.logger = logger;
            this.token = token;

            this.cancellationTokens = new Dictionary<string, CancellationTokenSource>();
        }

        public async Task StartDevicesAsync(List<IDevice> devices)
        {
            await Task.Run(
                async () =>
                {
                    if (devices == null || !devices.Any())
                    {
                        return;
                    }

                    var startDeviceTasks = new List<Task>();

                    foreach (var device in devices)
                    {
                        var deviceCancellationToken = new CancellationTokenSource();

                        startDeviceTasks.Add(device.StartAsync(deviceCancellationToken.Token));

                        cancellationTokens.Add(device.DeviceID, deviceCancellationToken);
                    }

                    await Task.WhenAll(startDeviceTasks);
                }, 
                this.token);
        }
        
        public void StopDevices(List<string> deviceIds)
        {
            foreach (string deviceId in deviceIds)
            {
                var cancellationToken = this.cancellationTokens[deviceId];

                if (cancellationToken != null)
                {
                    cancellationToken.Cancel();
                    this.cancellationTokens.Remove(deviceId);

                    this.logger.LogInfo("********** STOPPED DEVICE : {0} ********** ", deviceId);
                }
            }
        }

        public void StopAllDevices()
        {
            foreach (KeyValuePair<string, CancellationTokenSource> cancellationToken in this.cancellationTokens)
            {
                cancellationToken.Value.Cancel();
                this.logger.LogInfo("********** STOPPED DEVICE : {0} ********** ", cancellationToken.Key);
            }

            this.cancellationTokens.Clear();
        }
    }
}
