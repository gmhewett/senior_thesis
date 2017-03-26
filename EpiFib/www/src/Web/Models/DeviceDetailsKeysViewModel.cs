// <copyright file="DeviceDetailsKeysViewModel.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace Web.Models
{
    using Web.Security;

    public class DeviceDetailsKeysViewModel
    {
        public string DeviceId { get; set; }

        public bool IsAllowedToViewKeys => PermsChecker.HasPermission(Permission.ViewDeviceSecurityKeys);
    }
}