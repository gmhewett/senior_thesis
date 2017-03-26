// <copyright file="Startup.WebApi.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace Web
{
    using System.Web.Http;
    using Owin;

    public partial class Startup
    {
        public void ConfigureWebApi(IAppBuilder app)
        {
            HttpConfiguration.MapHttpAttributeRoutes();
            app.UseAutofacWebApi(HttpConfiguration);
            app.UseWebApi(HttpConfiguration);
        }
    }
}