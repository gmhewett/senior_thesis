// <copyright file="AppConfigRepository.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace Simulator.WebJob.SimulatorCore
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.Configuration;
    using Common.Models;
    using Common.Repository;
    using Simulator.WebJob.Properties;
    using Simulator.WebJob.SimulatorCore.Logging;

    public class AppConfigRepository : IVirtualDeviceStorage
    {
        private readonly string hostName;
        private readonly List<InitialDeviceConfig> devices;
        private readonly ILogger logger;

        public AppConfigRepository(IConfigurationProvider configProvider, ILogger logger)
        {
            this.devices = new List<InitialDeviceConfig>();
            this.hostName = configProvider.GetConfigurationSettingValue("iotHub.HostName");
            this.logger = logger;
        }

        public async Task<List<InitialDeviceConfig>> GetDeviceListAsync()
        {
            return await Task.Run(() =>
            {
                logger.LogInfo("********** READING DEVICES FROM APP.CONFIG ********** ");
                if (devices.Any())
                {
                    return devices;
                }

                StringCollection deviceList = Settings.Default.DeviceList;

                foreach (string device in deviceList)
                {
                    string[] deviceConfigElements = device.Split(',');
                    var deviceConfig = new InitialDeviceConfig();

                    if (deviceConfigElements.Length > 1)
                    {
                        deviceConfig.DeviceId = deviceConfigElements[0];
                        deviceConfig.HostName = hostName;
                        deviceConfig.Key = deviceConfigElements[1];

                        devices.Add(deviceConfig);
                    }
                }

                return devices;
            });
        }

        public Task<InitialDeviceConfig> GetDeviceAsync(string deviceId)
        {
            return Task.Run(() =>
            {
                return !devices.Any() ? null : devices.FirstOrDefault(x => x.DeviceId == deviceId);
            });
        }

        public Task AddOrUpdateDeviceAsync(InitialDeviceConfig deviceConfig)
        {
            return Task.Run(() =>
            {
                if (!devices.Any())
                {
                    return;
                }

                var device = devices.FirstOrDefault(x => x.DeviceId == deviceConfig.DeviceId);

                if (device != null)
                {
                    device.Key = deviceConfig.Key;
                    device.HostName = deviceConfig.HostName;
                }
                else
                {
                    devices.Add(deviceConfig);
                }
            });
        }

        public Task<bool> RemoveDeviceAsync(string deviceId)
        {
            return Task.Run<bool>(() =>
            {
                if (!devices.Any())
                {
                    return false;
                }

                var device = devices.FirstOrDefault(x => x.DeviceId == deviceId);

                return device != null && devices.Remove(device);
            });
        }
    }
}
