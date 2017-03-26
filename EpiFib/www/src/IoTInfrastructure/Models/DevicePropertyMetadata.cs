// <copyright file="DevicePropertyMetadata.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace IoTInfrastructure.Models
{
    public class DevicePropertyMetadata
    {
        public bool IsDisplayedForRegisteredDevices { get; set; }

        public bool IsDisplayedForUnregisteredDevices { get; set; }

        public bool IsEditable { get; set; }

        public string Name { get; set; }

        public PropertyType PropertyType { get; set; }
    }
}
