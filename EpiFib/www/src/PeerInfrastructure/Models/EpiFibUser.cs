// <copyright file="EpiFibUser.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace PeerInfrastructure.Models
{
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;

    public class EpiFibUser : IdentityUser
    {
        public string DeviceToken { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(
            UserManager<EpiFibUser> manager,
            string authenticationType)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
            
            // Add custom user claims here
            return userIdentity;
        }
    }
}