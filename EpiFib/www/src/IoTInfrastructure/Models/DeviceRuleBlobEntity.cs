// <copyright file="DeviceRuleBlobEntity.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace IoTInfrastructure.Models
{
    public class DeviceRuleBlobEntity
    {
        public DeviceRuleBlobEntity(string deviceId)
        {
            this.DeviceId = deviceId;
        }

        public string DeviceId { get; private set; }

        public double? Temperature { get; set; }

        public double? Humidity { get; set; }

        public string TemperatureRuleOutput { get; set; }

        public string HumidityRuleOutput { get; set; }
    }
}
