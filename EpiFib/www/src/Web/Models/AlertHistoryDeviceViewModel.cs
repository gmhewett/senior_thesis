// <copyright file="AlertHistoryDeviceViewModel.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace Web.Models
{
    public enum AlertHistoryDeviceStatus
    {
        AllClear = 0,
        Caution,
        Critical
    }

    public class AlertHistoryDeviceViewModel
    {
        public string DeviceId { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public AlertHistoryDeviceStatus Status { get; set; }
    }
}