// <copyright file="EditDevicePropertiesViewModel.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace Web.Models
{
    using System.Collections.Generic;
    using IoTInfrastructure.Models;

    public class EditDevicePropertiesViewModel
    {
        public string DeviceId { get; set; }

        public List<DevicePropertyValueModel> DevicePropertyValueModels { get; set; }
    }
}