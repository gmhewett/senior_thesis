// <copyright file="HtmlHelperExtensions.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace Web.Helpers
{
    using System.Web;
    using System.Web.Mvc;

    public static class HtmlHelperExtensions
    {
        public static IHtmlString JavaScriptString(this HtmlHelper htmlHelper, string message)
        {
            return htmlHelper.Raw(HttpUtility.JavaScriptStringEncode(message));
        }
    }
}