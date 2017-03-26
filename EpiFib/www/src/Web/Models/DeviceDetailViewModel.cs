// <copyright file="DeviceDetailViewModel.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace Web.Models
{
    using System.Collections.Generic;
    using Common.Models;
    using IoTInfrastructure.Models;
    using Web.Security;

    public class DeviceDetailViewModel
    {
        public string DeviceID { get; set; }

        public bool HasKeyViewingPerm => PermsChecker.HasPermission(Permission.ViewDeviceSecurityKeys);

        public bool HasConfigEditPerm => PermsChecker.HasPermission(Permission.EditDeviceMetadata);

        public bool? HubEnabledState { get; set; }

        public bool DeviceIsEnabled => this.HubEnabledState == true;

        public SecurityKeys IoTHubKeys { get; set; }

        public bool CanDisableDevice => PermsChecker.HasPermission(Permission.DisableEnableDevices);

        public bool CanRemoveDevice => PermsChecker.HasPermission(Permission.RemoveDevices);

        public bool IsDeviceEditEnabled => PermsChecker.HasPermission(Permission.EditDeviceMetadata);

        public bool CanAddRule => PermsChecker.HasPermission(Permission.EditRules);

        public List<DevicePropertyValueModel> DevicePropertyValueModels { get; set; }

        public bool IsCellular { get; set; }

        public string Iccid { get; set; }

        public DeviceDetailsKeysViewModel GetKeys()
        {
            return new DeviceDetailsKeysViewModel
            {
                DeviceId = this.DeviceID
            };
        }
    }
}