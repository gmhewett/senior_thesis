// <copyright file="EpiFibSignInManager.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace Web.Services
{
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.Owin;
    using Microsoft.Owin;
    using Microsoft.Owin.Security;
    using PeerInfrastructure.Models;

    public class EpiFibSignInManager : SignInManager<EpiFibUser, string>
    {
        public EpiFibSignInManager(EpiFibUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        public static EpiFibSignInManager Create(IdentityFactoryOptions<EpiFibSignInManager> options, IOwinContext context)
        {
            return new EpiFibSignInManager(context.GetUserManager<EpiFibUserManager>(), context.Authentication);
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(EpiFibUser user)
        {
            return user.GenerateUserIdentityAsync((EpiFibUserManager)UserManager, DefaultAuthenticationTypes.ApplicationCookie);
        }
    }
}