// <copyright file="AccountApiController.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace Web.ApiControllers
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Security.Claims;
    using System.Security.Cryptography;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Http;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Microsoft.AspNet.Identity.Owin;
    using Microsoft.Owin.Security;
    using Microsoft.Owin.Security.Cookies;
    using Microsoft.Owin.Security.OAuth;
    using PeerInfrastructure.Models;
    using Web.Models.Account;
    using Web.Providers;
    using Web.Results;
    using Web.Security;
    using Web.Services;

    [Authorize]
    [RoutePrefix("api/v1/Account")]
    public class AccountApiController : ApiController
    {
        private const string LocalLoginProvider = "Local";

        private EpiFibUserManager userManager;

        public AccountApiController()
        {
        }

        public AccountApiController(
            EpiFibUserManager userManager,
            ISecureDataFormat<AuthenticationTicket> accessTokenFormat)
        {
            this.UserManager = userManager;
            this.AccessTokenFormat = accessTokenFormat;
        }

        public EpiFibUserManager UserManager
        {
            get
            {
                return this.userManager ?? Request.GetOwinContext().GetUserManager<EpiFibUserManager>();
            }

            private set
            {
                this.userManager = value;
            }
        }

        public ISecureDataFormat<AuthenticationTicket> AccessTokenFormat { get; }

        private IAuthenticationManager Authentication => Request.GetOwinContext().Authentication;

        // GET api/v1/account/UserInfo
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [Route("UserInfo")]
        public UserInfoViewModel GetUserInfo()
        {
            ExternalLoginData externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);

            return new UserInfoViewModel
            {
                Email = User.Identity.GetUserName(),
                HasRegistered = externalLogin == null,
                LoginProvider = externalLogin?.LoginProvider
            };
        }

        // POST api/v1/account/Logout
        [Route("Logout")]
        public IHttpActionResult Logout()
        {
            this.Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
            return this.Ok();
        }

        // GET api/v1/account/ManageInfo?returnUrl=%2F&generateState=true
        [Route("ManageInfo")]
        public async Task<ManageInfoViewModel> GetManageInfo(string returnUrl, bool generateState = false)
        {
            EpiFibUser user = await this.UserManager.FindByIdAsync(User.Identity.GetUserId());

            if (user == null)
            {
                return null;
            }

            List<UserLoginInfoViewModel> logins = new List<UserLoginInfoViewModel>();

            foreach (IdentityUserLogin linkedAccount in user.Logins)
            {
                logins.Add(new UserLoginInfoViewModel
                {
                    LoginProvider = linkedAccount.LoginProvider,
                    ProviderKey = linkedAccount.ProviderKey
                });
            }

            if (user.PasswordHash != null)
            {
                logins.Add(new UserLoginInfoViewModel
                {
                    LoginProvider = LocalLoginProvider,
                    ProviderKey = user.UserName,
                });
            }

            return new ManageInfoViewModel
            {
                LocalLoginProvider = LocalLoginProvider,
                Email = user.UserName,
                Logins = logins,
                ExternalLoginProviders = this.GetExternalLogins(returnUrl, generateState)
            };
        }

        // POST api/v1/account/ChangePassword
        [Route("ChangePassword")]
        public async Task<IHttpActionResult> ChangePassword(ChangePasswordBindingModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            IdentityResult result = await this.UserManager.ChangePasswordAsync(
                User.Identity.GetUserId(), 
                model.OldPassword,
                model.NewPassword);
            
            return !result.Succeeded ? this.GetErrorResult(result) : this.Ok();
        }

        // POST api/v1/account/SetPassword
        [Route("SetPassword")]
        public async Task<IHttpActionResult> SetPassword(SetPasswordBindingModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            IdentityResult result = await this.UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);

            return !result.Succeeded ? this.GetErrorResult(result) : this.Ok();
        }

        // POST api/v1/account/AddExternalLogin
        [Route("AddExternalLogin")]
        public async Task<IHttpActionResult> AddExternalLogin(AddExternalLoginBindingModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            this.Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);

            AuthenticationTicket ticket = this.AccessTokenFormat.Unprotect(model.ExternalAccessToken);

            if (ticket?.Identity == null ||
                (ticket.Properties?.ExpiresUtc != null && ticket.Properties.ExpiresUtc.Value < DateTimeOffset.UtcNow))
            {
                return this.BadRequest("External login failure.");
            }

            ExternalLoginData externalData = ExternalLoginData.FromIdentity(ticket.Identity);

            if (externalData == null)
            {
                return this.BadRequest("The external login is already associated with an account.");
            }

            IdentityResult result = await this.UserManager.AddLoginAsync(
                User.Identity.GetUserId(),
                new UserLoginInfo(externalData.LoginProvider, externalData.ProviderKey));

            return !result.Succeeded ? this.GetErrorResult(result) : this.Ok();
        }

        // POST api/v1/account/RemoveLogin
        [Route("RemoveLogin")]
        public async Task<IHttpActionResult> RemoveLogin(RemoveLoginBindingModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            IdentityResult result;

            if (model.LoginProvider == LocalLoginProvider)
            {
                result = await this.UserManager.RemovePasswordAsync(User.Identity.GetUserId());
            }
            else
            {
                result = await this.UserManager.RemoveLoginAsync(
                    User.Identity.GetUserId(),
                    new UserLoginInfo(model.LoginProvider, model.ProviderKey));
            }

            return !result.Succeeded ? this.GetErrorResult(result) : this.Ok();
        }

        // GET api/v1/account/ExternalLogin
        [OverrideAuthentication]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalCookie)]
        [AllowAnonymous]
        [Route("ExternalLogin", Name = "ExternalLogin")]
        public async Task<IHttpActionResult> GetExternalLogin(string provider, string error = null)
        {
            if (error != null)
            {
                return this.Redirect(Url.Content("~/") + "#error=" + Uri.EscapeDataString(error));
            }

            if (!User.Identity.IsAuthenticated)
            {
                return new ChallengeResult(provider, this);
            }

            ExternalLoginData externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);

            if (externalLogin == null)
            {
                return this.InternalServerError();
            }

            if (externalLogin.LoginProvider != provider)
            {
                this.Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
                return new ChallengeResult(provider, this);
            }

            EpiFibUser user = await this.UserManager.FindAsync(
                new UserLoginInfo(
                    externalLogin.LoginProvider,
                    externalLogin.ProviderKey));

            bool hasRegistered = user != null;

            if (hasRegistered)
            {
                this.Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
                
                 ClaimsIdentity oAuthIdentity = await user.GenerateUserIdentityAsync(
                     this.UserManager,
                    OAuthDefaults.AuthenticationType);
                ClaimsIdentity cookieIdentity = await user.GenerateUserIdentityAsync(
                    this.UserManager,
                    CookieAuthenticationDefaults.AuthenticationType);

                AuthenticationProperties properties = ApplicationOAuthProvider.CreateProperties(user.UserName);
                this.Authentication.SignIn(properties, oAuthIdentity, cookieIdentity);
            }
            else
            {
                IEnumerable<Claim> claims = externalLogin.GetClaims();
                ClaimsIdentity identity = new ClaimsIdentity(claims, OAuthDefaults.AuthenticationType);
                this.Authentication.SignIn(identity);
            }

            return this.Ok();
        }

        // GET api/v1/account/ExternalLogins?returnUrl=%2F&generateState=true
        [AllowAnonymous]
        [Route("ExternalLogins")]
        public IEnumerable<ExternalLoginViewModel> GetExternalLogins(string returnUrl, bool generateState = false)
        {
            IEnumerable<AuthenticationDescription> descriptions = this.Authentication.GetExternalAuthenticationTypes();
            List<ExternalLoginViewModel> logins = new List<ExternalLoginViewModel>();

            string state;

            if (generateState)
            {
                const int StrengthInBits = 256;
                state = RandomOAuthStateGenerator.Generate(StrengthInBits);
            }
            else
            {
                state = null;
            }

            foreach (AuthenticationDescription description in descriptions)
            {
                ExternalLoginViewModel login = new ExternalLoginViewModel
                {
                    Name = description.Caption,
                    Url = Url.Route(
                        "ExternalLogin", 
                        new
                        {
                            provider = description.AuthenticationType,
                            response_type = "token",
                            client_id = Startup.PublicClientId,
                            redirect_uri = new Uri(Request.RequestUri, returnUrl).AbsoluteUri,
                            state = state
                        }),
                        State = state
                };
                logins.Add(login);
            }

            return logins;
        }

        // POST api/v1/account/Register
        [AllowAnonymous]
        [Route("Register")]
        public async Task<IHttpActionResult> Register(RegisterBindingModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var user = new EpiFibUser { UserName = model.Email, Email = model.Email };

            IdentityResult userResult = await this.UserManager.CreateAsync(user, model.Password);

            if (userResult.Succeeded)
            {
                IdentityResult roleResult = await this.UserManager.AddToRoleAsync(user.Id, Role.ReadonlyRoleName);

                return roleResult.Succeeded ? this.Ok() : this.GetErrorResult(roleResult);
            }

            return this.GetErrorResult(userResult);
        }

        [HttpPost]
        [Route("DeviceToken")]
        public async Task<IHttpActionResult> DeviceToken(DeviceTokenBindingModel deviceTokenModel)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            if (string.IsNullOrWhiteSpace(deviceTokenModel.DeviceToken))
            {
                return this.BadRequest($"{nameof(deviceTokenModel.DeviceToken)} null.");
            }

            EpiFibUser user = await this.UserManager.FindByIdAsync(User.Identity.GetUserId());
            user.DeviceToken = deviceTokenModel.DeviceToken;

            IdentityResult result = await this.UserManager.UpdateAsync(user);
            return result.Succeeded ? (IHttpActionResult)this.Ok() : this.BadRequest("Could not update user.");
        }

        // POST api/v1/account/RegisterExternal
        [OverrideAuthentication]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [Route("RegisterExternal")]
        public async Task<IHttpActionResult> RegisterExternal(RegisterExternalBindingModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var info = await this.Authentication.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return this.InternalServerError();
            }

            var user = new EpiFibUser() { UserName = model.Email, Email = model.Email };

            IdentityResult result = await this.UserManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                return this.GetErrorResult(result);
            }

            result = await this.UserManager.AddLoginAsync(user.Id, info.Login);
            return !result.Succeeded ? this.GetErrorResult(result) : this.Ok();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && this.UserManager != null)
            {
                this.UserManager.Dispose();
                this.UserManager = null;
            }

            base.Dispose(disposing);
        }

        #region Helpers

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return this.InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        this.ModelState.AddModelError(string.Empty, error);
                    }
                }

                if (this.ModelState.IsValid)
                {
                    // No this.ModelState errors are available to send, so just return an empty BadRequest.
                    return this.BadRequest();
                }

                return this.BadRequest(this.ModelState);
            }

            return null;
        }

        private static class RandomOAuthStateGenerator
        {
            private static readonly RandomNumberGenerator Random = new RNGCryptoServiceProvider();

            public static string Generate(int strengthInBits)
            {
                const int BitsPerByte = 8;

                if (strengthInBits % BitsPerByte != 0)
                {
                    throw new ArgumentException("strengthInBits must be evenly divisible by 8.", nameof(strengthInBits));
                }

                int strengthInBytes = strengthInBits / BitsPerByte;

                byte[] data = new byte[strengthInBytes];
                Random.GetBytes(data);
                return HttpServerUtility.UrlTokenEncode(data);
            }
        }

        private class ExternalLoginData
        {
            public string LoginProvider { get; private set; }

            public string ProviderKey { get; private set; }

            private string UserName { get; set; }

            public static ExternalLoginData FromIdentity(ClaimsIdentity identity)
            {
                Claim providerKeyClaim = identity?.FindFirst(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(providerKeyClaim?.Issuer) || string.IsNullOrEmpty(providerKeyClaim.Value))
                {
                    return null;
                }

                if (providerKeyClaim.Issuer == ClaimsIdentity.DefaultIssuer)
                {
                    return null;
                }

                return new ExternalLoginData
                {
                    LoginProvider = providerKeyClaim.Issuer,
                    ProviderKey = providerKeyClaim.Value,
                    UserName = identity.FindFirstValue(ClaimTypes.Name)
                };
            }

            public IList<Claim> GetClaims()
            {
                IList<Claim> claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.NameIdentifier, this.ProviderKey, null, this.LoginProvider));

                if (this.UserName != null)
                {
                    claims.Add(new Claim(ClaimTypes.Name, this.UserName, null, this.LoginProvider));
                }

                return claims;
            }
        }
        #endregion
    }
}
