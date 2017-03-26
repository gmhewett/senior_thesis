// <copyright file="DeviceQueryResult.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace IoTInfrastructure.Models
{
    using System.Collections.Generic;
    using Common.Models;

    public class DeviceQueryResult
    {
        public int TotalDeviceCount { get; set; }

        public int TotalFilteredCount { get; set; }

        public List<DeviceModel> Results { get; set; }
    }
}
