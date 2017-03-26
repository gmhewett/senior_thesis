// <copyright file="Startup.Auth.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace Web
{
    using System;
    using System.Web;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.Owin;
    using Microsoft.Owin;
    using Microsoft.Owin.Security.Cookies;
    using Microsoft.Owin.Security.OAuth;
    using Owin;
    using PeerInfrastructure.Models;
    using PeerInfrastructure.Repository;
    using Web.Providers;
    using Web.Services;

    public partial class Startup
    {
        public static string PublicClientId { get; private set; }

        public void ConfigureAuth(IAppBuilder app)
        {
            app.CreatePerOwinContext(EpiFibDbContext.Create);
            app.CreatePerOwinContext<EpiFibUserManager>(EpiFibUserManager.Create);
            app.CreatePerOwinContext<EpiFibSignInManager>(EpiFibSignInManager.Create);

            var cookieOptions = new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                Provider = new CookieAuthenticationProvider
                {
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<EpiFibUserManager, EpiFibUser>(
                        validateInterval: TimeSpan.FromMinutes(30),
                        regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(
                            manager,
                            DefaultAuthenticationTypes.ApplicationCookie)),
                    OnApplyRedirect = ctx =>
                    {
                        if (!IsApiRequest(ctx.Request))
                        {
                            ctx.Response.Redirect(ctx.RedirectUri);
                        }
                    }
                }
            };

            app.UseCookieAuthentication(cookieOptions);
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);
            app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));
            app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);

            PublicClientId = "self";
            app.UseOAuthBearerTokens(new OAuthAuthorizationServerOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ExternalBearer,
                TokenEndpointPath = new PathString("/api/v1/Token"),
                Provider = new ApplicationOAuthProvider(PublicClientId),
                AuthorizeEndpointPath = new PathString("/api/v1/account/ExternalLogin"),
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(14),
                AllowInsecureHttp = true
            });

            ////app.UseMicrosoftAccountAuthentication(
            ////    clientId: "",
            ////    clientSecret: "");

            ////app.UseTwitterAuthentication(
            ////    consumerKey: "",
            ////    consumerSecret: "");

            ////app.UseFacebookAuthentication(
            ////    appId: "",
            ////    appSecret: "");

            ////app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
            ////{
            ////    ClientId = "",
            ////    ClientSecret = ""
            ////});
        }

        private static bool IsApiRequest(IOwinRequest request)
        {
            string apiPath = VirtualPathUtility.ToAbsolute("~/api/");
            return request.Uri.LocalPath.StartsWith(apiPath);
        }
    }
}
