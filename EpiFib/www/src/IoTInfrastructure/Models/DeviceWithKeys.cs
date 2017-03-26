// <copyright file="DeviceWithKeys.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace IoTInfrastructure.Models
{
    using Common.Models;

    public class DeviceWithKeys
    {
        public DeviceWithKeys(DeviceModel device, SecurityKeys securityKeys)
        {
            this.Device = device;
            this.SecurityKeys = securityKeys;
        }

        public DeviceModel Device { get; set; }

        public SecurityKeys SecurityKeys { get; set; }   
    }
}
