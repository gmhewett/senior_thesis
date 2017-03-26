// <copyright file="Startup.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

using Microsoft.Owin;

[assembly: OwinStartup(typeof(Web.Startup))]

namespace Web
{
    using System.Web.Http;
    using Owin;

    public partial class Startup
    {
        public static HttpConfiguration HttpConfiguration { get; private set; }

        public void Configuration(IAppBuilder app)
        {
            this.ConfigureAuth(app);
            this.ConfigureAutofac(app);
            this.ConfigureWebApi(app);
            this.ConfigureJson(app);
        }
    }
}
