// <copyright file="ApiRequirePermissionAttribute.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace Web.Security
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http;

    public class ApiRequirePermissionAttribute : AuthorizeAttribute
    {
        public ApiRequirePermissionAttribute(params Permission[] values)
        {
            if (values != null)
            {
                this.Permissions = values.ToList();
            }
        }

        public List<Permission> Permissions { get; set; }

        protected override bool IsAuthorized(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            return PermsChecker.HasPermission(this.Permissions);
        }
    }
}