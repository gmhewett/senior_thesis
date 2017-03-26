// <copyright file="DeviceTelemetryFieldModel.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace IoTInfrastructure.Models
{
    public class DeviceTelemetryFieldModel
    {
        public string DisplayName { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }
    }
}
