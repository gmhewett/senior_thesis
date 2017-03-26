// <copyright file="DeviceListLocationsModel.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace IoTInfrastructure.Models
{
    using System.Collections.Generic;

    public class DeviceListLocationsModel
    {
        public List<DeviceLocationModel> DeviceLocationList { get; set; }

        public double MinimumLatitude { get; set; }

        public double MaximumLatitude { get; set; }

        public double MinimumLongitude { get; set; }

        public double MaximumLongitude { get; set; }
    }
}
