// <copyright file="ActionsController.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace Web.Controllers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using IoTInfrastructure.Models;
    using IoTInfrastructure.Services;
    using Web.Models;
    using Web.Security;

    public class ActionsController : Controller
    {
        private readonly IActionMappingService actionMappingService;
        private readonly IActionService actionService;

        public ActionsController(IActionMappingService actionMappingService, IActionService actionService)
        {
            this.actionMappingService = actionMappingService;
            this.actionService = actionService;
        }

        [RequirePermission(Permission.ViewActions)]
        public ActionResult Index()
        {
            var model = new ActionPropertiesViewModel();
            return this.View(model);
        }

        [HttpGet]
        [RequirePermission(Permission.AssignAction)]
        public async Task<ActionResult> GetAvailableLogicAppActions()
        {
            List<SelectListItem> actionIds = await this.ActionListItems();

            var actionPropertiesModel = new ActionPropertiesViewModel
            {
                UpdateActionModel = new UpdateActionViewModel
                {
                    ActionSelectList = actionIds,
                }
            };

            return this.PartialView("_ActionProperties", actionPropertiesModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(Permission.AssignAction)]
        public async Task<ActionMapping> UpdateAction(string ruleOutput, string actionId)
        {
            var actionMapping = new ActionMapping();

            actionMapping.RuleOutput = ruleOutput;
            actionMapping.ActionId = actionId;
            await this.actionMappingService.SaveMappingAsync(actionMapping);
            return actionMapping;
        }

        private async Task<List<SelectListItem>> ActionListItems()
        {
            List<string> actionIds = await this.actionService.GetAllActionIdsAsync();
            if (actionIds != null)
            {
                var actionListItems = new List<SelectListItem>();
                foreach (string actionId in actionIds)
                {
                    var item = new SelectListItem();
                    item.Value = actionId;
                    item.Text = actionId;
                    actionListItems.Add(item);
                }

                return actionListItems;
            }

            return new List<SelectListItem>();
        }
    }
}