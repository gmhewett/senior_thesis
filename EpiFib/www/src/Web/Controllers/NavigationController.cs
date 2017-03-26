// <copyright file="NavigationController.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace Web.Controllers
{
    using System.Web.Mvc;
    using Web.Helpers;
    using Web.Navigation;

    public class NavigationController : Controller
    {
        public ActionResult NavigationMenu()
        {
            var navigationMenu = new NavigationMenu();

            string action = ControllerContext.ParentActionViewContext.RouteData.Values["action"].ToString();
            string controller = ControllerContext.ParentActionViewContext.RouteData.Values["controller"].ToString();

            NavigationHelper.ApplySelection(navigationMenu.NavigationMenuItems, controller, action);

            return this.PartialView("_NavigationMenu", navigationMenu.NavigationMenuItems);
        }

        public ActionResult NavigationSubmenu()
        {
            string action = ControllerContext.ParentActionViewContext.RouteData.Values["action"].ToString();
            string controller = ControllerContext.ParentActionViewContext.RouteData.Values["controller"].ToString();

            var menuItems = NavigationHelper.GetSubnavigationItemsForController(controller);
            if ((menuItems == null) || (menuItems.Count == 0))
            {
                return new EmptyResult();
            }

            NavigationHelper.ApplySubmenuSelection(menuItems, controller, action);

            return this.PartialView("_NavigationSubmenu", menuItems);
        }
    }
}