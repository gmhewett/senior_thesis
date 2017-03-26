// <copyright file="DeviceCreatorHelper.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace Common.Helpers
{
    using System;
    using System.Collections.Generic;
    using Common.Models;

    public static class DeviceCreatorHelper
    {
        public static DeviceModel BuildDeviceStructure(string deviceId, bool isSimulated, string iccid)
        {
            DeviceModel device = new DeviceModel();

            InitializeDeviceProperties(device, deviceId, isSimulated);
            InitializeSystemProperties(device, iccid);

            device.Commands = new List<Command>();
            device.CommandHistory = new List<CommandHistory>();
            device.IsSimulatedDevice = isSimulated;

            return device;
        }

        private static void InitializeDeviceProperties(DeviceModel device, string deviceId, bool isSimulated)
        {
            DeviceProperties deviceProps = new DeviceProperties
            {
                DeviceID = deviceId,
                HubEnabledState = null,
                CreatedTime = DateTime.UtcNow,
                DeviceState = "normal",
                UpdatedTime = DateTime.UtcNow
            };

            device.DeviceProperties = deviceProps;
            device.IsSimulatedDevice = isSimulated;
        }

        private static void InitializeSystemProperties(DeviceModel device, string iccid)
        {
            var systemProps = new SystemProperties
            {
                ICCID = iccid
            };

            device.SystemProperties = systemProps;
        }
    }
}
