// <copyright file="DevicePropertyValueModel.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace IoTInfrastructure.Models
{
    public class DevicePropertyValueModel
    {
        public int DisplayOrder { get; set; }

        public bool IsEditable { get; set; }

        public bool IsIncludedWithUnregisteredDevices { get; set; }

        public string Name { get; set; }

        public PropertyType PropertyType { get; set; }

        public string Value { get; set; }
    }
}
