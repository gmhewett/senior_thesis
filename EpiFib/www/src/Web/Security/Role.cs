// <copyright file="Role.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace Web.Security
{
    using System;

    public static class Role
    {
        public const string AdminRoleName = "admin";

        public const string ReadonlyRoleName = "readonly";

        public static string GetValidRoleName(string roleName)
        {
            if (string.Equals(roleName, AdminRoleName, StringComparison.InvariantCultureIgnoreCase))
            {
                return AdminRoleName;
            }

            if (string.Equals(roleName, ReadonlyRoleName, StringComparison.InvariantCultureIgnoreCase))
            {
                return ReadonlyRoleName;
            }

            return string.Empty;
        }
    }
}