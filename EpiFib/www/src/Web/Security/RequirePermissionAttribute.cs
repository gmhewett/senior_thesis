// <copyright file="RequirePermissionAttribute.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace Web.Security
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Web;
    using System.Web.Mvc;

    public class RequirePermissionAttribute : AuthorizeAttribute
    {
        public RequirePermissionAttribute(params Permission[] values)
        {
            if (values != null)
            {
                this.Permissions = values.ToList();
            }
        }

        public List<Permission> Permissions { get; set; }
        
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            bool isAuthorized = base.AuthorizeCore(httpContext);

            return isAuthorized && PermsChecker.HasPermission(this.Permissions);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.Request.IsAuthenticated)
            {
                filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }
            else
            {
                base.HandleUnauthorizedRequest(filterContext);
            }
        }
    }
}