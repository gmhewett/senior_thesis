// <copyright file="Global.asax.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace WebJobHost
{
    using System.Diagnostics;

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            Trace.TraceInformation("WebJobHost starting...");
        }
    }
}