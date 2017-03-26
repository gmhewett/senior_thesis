// <copyright file="AlertHistoryResultsViewModel.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace Web.Models
{
    using System.Collections.Generic;
    using IoTInfrastructure.Models;

    public class AlertHistoryResultsViewModel
    {
        public int TotalAlertCount { get; set; }

        public int TotalFilteredCount { get; set; }

        public List<AlertHistoryItemModel> Data { get; set; }

        public List<AlertHistoryDeviceViewModel> Devices
        {
            get;
            set;
        }

        public double MaxLatitude
        {
            get;
            set;
        }

        public double MaxLongitude
        {
            get;
            set;
        }

        public double MinLatitude
        {
            get;
            set;
        }

        public double MinLongitude
        {
            get;
            set;
        }
    }
}