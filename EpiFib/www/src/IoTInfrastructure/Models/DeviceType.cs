// <copyright file="DeviceType.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace IoTInfrastructure.Models
{
    using System;

    public class DeviceType
    {
        public int DeviceTypeId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public Uri ImageUrl { get; set; }

        public string InstructionsUrl { get; set; }

        public bool IsSimulatedDevice { get; set; }
    }
}
