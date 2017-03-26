// <copyright file="DeviceLocationModel.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace IoTInfrastructure.Models
{
    public class DeviceLocationModel
    {
        public string DeviceId { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }
    }
}
