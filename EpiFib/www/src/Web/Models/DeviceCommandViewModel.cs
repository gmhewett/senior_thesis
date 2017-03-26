// <copyright file="DeviceCommandViewModel.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace Web.Models
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using Common.Models;
    using Web.Security;

    public class DeviceCommandViewModel
    {
        [DisplayName("Command")]
        public Command Command { get; set; }

        public List<CommandHistory> CommandHistory { get; set; }

        public string DeviceId { get; set; }

        public bool? DeviceIsEnabled { get; set; }

        public string CommandsJson { get; set; }

        public bool SupportDeviceCommand => PermsChecker.HasPermission(Permission.SendCommandToDevices);

        public SendCommandViewModel SendCommandModel { get; set; }
    }
}