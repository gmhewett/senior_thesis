// <copyright file="BundleConfig.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace Web
{
    using System.Web.Optimization;

    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                "~/Scripts/jquery-3.1.1.min.js",
                "~/Scripts/powerbi-visuals.all.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                "~/Scripts/jquery.unobtrusive*",
                "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/jquerytable").Include(
                "~/Scripts/DataTables/jquery.dataTables.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                "~/Scripts/jquery-ui-1.12.1.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                "~/Scripts/bootstrap.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/js-cookie").Include(
                "~/Scripts/js-cookie/js.cookie-2.1.3.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/moment").Include(
                "~/Scripts/moment-with-locales.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/dialog").Include(
                "~/Scripts/dialog.js"));

            bundles.Add(new StyleBundle("~/content/css/vendor").Include(
               "~/content/styles/datatables.css",
               "~/content/themes/base/core.css",
               "~/content/themes/base/dialog.css",
               "~/content/styles/visuals.min.css"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/content/styles/main.css"));
        }
    }
}
