// <copyright file="PermsChecker.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace Web.Security
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    public static class PermsChecker
    {
        private static readonly RolePermissions RolePermissions;

        static PermsChecker()
        {
            RolePermissions = new RolePermissions();
        }

        public static bool HasPermission(Permission permission)
        {
            return RolePermissions.HasPermission(permission, new HttpContextWrapper(HttpContext.Current));
        }

        public static bool HasPermission(List<Permission> permissions)
        {
            var httpContext = new HttpContextWrapper(HttpContext.Current);

            if (permissions == null || !permissions.Any())
            {
                return true;
            }

            // return true only if the user has ALL permissions
            return permissions
                    .Select(p => RolePermissions.HasPermission(p, httpContext))
                    .All(val => val);
        }
    }
}