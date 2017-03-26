// <copyright file="NavigationHelper.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace Web.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Common.Helpers;
    using Resources;
    using Web.Navigation;
    using Web.Security;

    public static class NavigationHelper
    {
        public static void ApplySelection(
            IEnumerable<NavigationMenuItem> menuItems,
            string controllerName,
            string actionName)
        {
            if (menuItems == null)
            {
                throw new ArgumentNullException("menuItems");
            }

            var getIsSubmenuItem = ProduceGetIsSubnav();

            menuItems = menuItems.Where(t => (t != null) && (t.Controller == controllerName));
            foreach (var navigationMenuItem in menuItems)
            {
                if (navigationMenuItem.Action == actionName || getIsSubmenuItem(controllerName, actionName))
                {
                    navigationMenuItem.Selected = true;
                    navigationMenuItem.Class = string.Format(CultureInfo.InvariantCulture, "{0} {1}", navigationMenuItem.Class, "selected");
                }
            }
        }

        public static void ApplySubmenuSelection(
            IEnumerable<NavigationMenuItem> menuItems,
            string controllerName,
            string actionName)
        {
            EFGuard.NotNull(menuItems, nameof(menuItems));

            menuItems = menuItems.Where(t => t != null);
            foreach (var navigationMenuItem in menuItems)
            {
                if (navigationMenuItem.Controller == controllerName && navigationMenuItem.Action == actionName)
                {
                    navigationMenuItem.Selected = true;
                    navigationMenuItem.Class = string.Format(CultureInfo.InvariantCulture, "{0} {1}", navigationMenuItem.Class, "selected");
                }
            }
        }

        /// <summary>
        /// Gets the NavigationMenuItem that describe a controller's
        /// subnavigation items.
        /// </summary>
        /// <param name="controllerName">
        /// The name of the controller.
        /// </param>
        /// <returns>
        /// NavigationMenuItems that describe a controller's subnavigation items.
        /// </returns>
        public static List<NavigationMenuItem> GetSubnavigationItemsForController(
            string controllerName)
        {
            var listItems = new List<NavigationMenuItem>();
            if (controllerName == "Advanced")
            {
                listItems = GetAdvancedControllerSubmenuItems();
            }

            listItems = listItems.Where(
                t => (t != null) && PermsChecker.HasPermission(t.MinimumPermission)).ToList();

            return listItems;
        }

        private static List<NavigationMenuItem> GetAdvancedControllerSubmenuItems()
        {
            var items = new List<NavigationMenuItem>
            {
                new NavigationMenuItem()
                {
                    Action = "CellularConn",
                    Controller = "Advanced",
                    MinimumPermission = Permission.CellularConn,
                    Text = Strings.CellularConn,
                    Selected = true
                }
            };

            return items;
        }

        private static Func<string, string, bool> ProduceGetIsSubnav()
        {
            var getControllerSubnavs =
                FunctionalHelper.Memoize<string, HashSet<string>>(
                    (controllerName) =>
                    {
                        IEnumerable<NavigationMenuItem> navItems = GetSubnavigationItemsForController(controllerName);

                        if (navItems == null)
                        {
                            navItems = new NavigationMenuItem[0];
                        }

                        navItems = navItems.Where(
                            t =>
                                (t != null) &&
                                !string.IsNullOrEmpty(t.Action));

                        return new HashSet<string>(navItems.Select(t => t.Action));
                    });

            return (controllerName, actionName) =>
            {
                if (string.IsNullOrEmpty(actionName))
                {
                    return false;
                }

                return getControllerSubnavs(controllerName).Contains(actionName);
            };
        }
    }
}