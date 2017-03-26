// <copyright file="SendCommandViewModel.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace Web.Models
{
    using System.Collections.Generic;
    using System.Web.Mvc;
    using Common.Models;

    public class SendCommandViewModel
    {
        public Command Command { get; set; }

        public IList<SelectListItem> CommandSelectList { get; set; }

        public string DeviceId { get; set; }

        public bool CanSendDeviceCommands { get; set; }

        public bool HasCommands => this.CommandSelectList?.Count > 0;
    }
}