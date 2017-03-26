// <copyright file="RolePermissions.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace Web.Security
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    public class RolePermissions
    {
        private readonly Dictionary<Permission, HashSet<string>> rolePermissions;
        private readonly List<string> allRoles;

        public RolePermissions()
        {
            this.allRoles = new List<string>
                {
                    Role.AdminRoleName,
                    Role.ReadonlyRoleName
                };

            this.rolePermissions = new Dictionary<Permission, HashSet<string>>();
            this.DefineRoles();
        }

        public bool HasPermission(Permission permission, HttpContextBase httpContext)
        {
            // get the list of roles that the user must have some overlap with to have the permission
            HashSet<string> rolesRequired = this.rolePermissions[permission];

            if (this.allRoles.Any(role => httpContext.User.IsInRole(role) && rolesRequired.Contains(role)))
            {
                return true;
            }

            // fallback for no roles -- give them at least Read Only status
            bool userHasAtLeastOneRole = this.allRoles.Any(role => httpContext.User.IsInRole(role));

            if (!userHasAtLeastOneRole)
            {
                if (rolesRequired.Contains(Role.ReadonlyRoleName))
                {
                    return true;
                }
            }

            return false;
        }

        private void DefineRoles()
        {
            this.AssignRolesToPermission(
                Permission.ViewDevices,
                Role.ReadonlyRoleName,
                Role.AdminRoleName);

            this.AssignRolesToPermission(
                Permission.ViewActions,
                Role.ReadonlyRoleName,
                Role.AdminRoleName);

            this.AssignRolesToPermission(
                Permission.AssignAction,
                Role.AdminRoleName);

            this.AssignRolesToPermission(
                Permission.DisableEnableDevices,
                Role.AdminRoleName);

            this.AssignRolesToPermission(
                Permission.EditDeviceMetadata,
                Role.AdminRoleName);

            this.AssignRolesToPermission(
                Permission.AddDevices,
                Role.AdminRoleName);

            this.AssignRolesToPermission(
                Permission.RemoveDevices,
                Role.AdminRoleName);

            this.AssignRolesToPermission(
                Permission.SendCommandToDevices,
                Role.AdminRoleName);

            this.AssignRolesToPermission(
                Permission.ViewDeviceSecurityKeys,
                Role.AdminRoleName);

            this.AssignRolesToPermission(
                Permission.ViewRules,
                Role.ReadonlyRoleName,
                Role.AdminRoleName);

            this.AssignRolesToPermission(
                Permission.ViewTelemetry,
                Role.ReadonlyRoleName,
                Role.AdminRoleName);

            this.AssignRolesToPermission(
                Permission.EditRules,
                Role.AdminRoleName);

            this.AssignRolesToPermission(
                Permission.DeleteRules,
                Role.AdminRoleName);

            this.AssignRolesToPermission(
                Permission.HealthBeat,
                Role.AdminRoleName);

            this.AssignRolesToPermission(
                Permission.LogicApps,
                Role.AdminRoleName);

            this.AssignRolesToPermission(
                Permission.CellularConn,
                Role.AdminRoleName);
        }

        private void AssignRolesToPermission(Permission permission, params string[] roles)
        {
            var rolesHashSet = new HashSet<string>();

            // add each role that grants this permission to the set of granting permissions
            foreach (string role in roles)
            {
                rolesHashSet.Add(role);
            }

            // add the permission and granting roles to the data structure
            this.rolePermissions.Add(permission, rolesHashSet);
        }
    }
}