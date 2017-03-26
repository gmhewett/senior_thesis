// <copyright file="PrincipalHelper.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace Web.Security
{
    using System.Linq;
    using System.Security.Claims;
    using System.Security.Principal;

    public static class PrincipalHelper
    {
        public static string GetEmailAddress(IPrincipal principal)
        {
            if (principal.Identity.Name != null)
            {
                return principal.Identity.Name;
            }

            var claimsPrincipal = principal as ClaimsPrincipal;

            var emailAddressClaim = claimsPrincipal?.Claims?.SingleOrDefault(c => c.Type == ClaimTypes.Email);

            return emailAddressClaim == null ? string.Empty : emailAddressClaim.Value;
        }
    }
}