// <copyright file="CultureController.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace Web.Controllers
{
    using System;
    using System.Web;
    using System.Web.Mvc;
    using Common.Configuration;
    using Common.Helpers;

    public class CultureController : Controller
    {
        private readonly string cultureCookieName;

        public CultureController(IConfigurationProvider configProvider)
        {
            EFGuard.NotNull(configProvider, nameof(configProvider));

            this.cultureCookieName = configProvider.GetConfigurationSettingValue("web.CultureCookieName");
        }
        
        [Route("culture/{cultureName}")]
        public ActionResult SetCulture(string cultureName)
        {
            // Save culture in a cookie
            HttpCookie cookie = this.Request.Cookies[this.cultureCookieName];

            if (cookie != null)
            {
                cookie.Value = cultureName; // update cookie value
            }
            else
            {
                cookie = new HttpCookie(this.cultureCookieName)
                {
                    Value = cultureName,
                    Expires = DateTime.Now.AddYears(1)
                };
            }

            Response.Cookies.Add(cookie);

            return this.RedirectToAction("Index", "Dashboard");
        }
    }
}