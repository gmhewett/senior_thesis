// <copyright file="EpiFibUserManager.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace Web.Services
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Microsoft.AspNet.Identity.Owin;
    using Microsoft.Owin;
    using PeerInfrastructure.Models;
    using PeerInfrastructure.Repository;

    public class EpiFibUserManager : UserManager<EpiFibUser>
    {
        public EpiFibUserManager(IUserStore<EpiFibUser> store)
            : base(store)
        {
        }

        public static EpiFibUserManager Create(IdentityFactoryOptions<EpiFibUserManager> options, IOwinContext context)
        {
            var manager = new EpiFibUserManager(new UserStore<EpiFibUser>(context.Get<EpiFibDbContext>()));

            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<EpiFibUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = true,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
            };

            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = new DataProtectorTokenProvider<EpiFibUser>(dataProtectionProvider.Create("ASP.NET Identity"));
            }

            return manager;
        }
    }
}