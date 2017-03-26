// <copyright file="InitialDeviceConfig.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace Common.Models
{
    public class InitialDeviceConfig
    {
        public string HostName { get; set; }

        public string DeviceId { get; set; }

        public string Key { get; set; }
    }
}
