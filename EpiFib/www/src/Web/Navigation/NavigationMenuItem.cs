// <copyright file="NavigationMenuItem.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace Web.Navigation
{
    using System.Collections.Generic;
    using Web.Security;

    public class NavigationMenuItem
    {
        public string Text { get; set; }

        public string Action { get; set; }

        public string Controller { get; set; }

        public bool Selected { get; set; }

        public string Class { get; set; }

        public List<NavigationMenuItem> Children { get; set; }

        public Permission MinimumPermission { get; set; }
    }
}