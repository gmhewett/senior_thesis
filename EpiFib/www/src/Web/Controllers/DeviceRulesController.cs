// <copyright file="DeviceRulesController.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace Web.Controllers
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using Common.Models;
    using IoTInfrastructure.Models;
    using IoTInfrastructure.Services;
    using Newtonsoft.Json;
    using Resources;
    using Web.Models;
    using Web.Security;

    public class DeviceRulesController : Controller
    {
        private readonly IDeviceRulesService deviceRulesService;

        public DeviceRulesController(IDeviceRulesService deviceRulesService)
        {
            this.deviceRulesService = deviceRulesService;
        }

        [RequirePermission(Permission.ViewRules)]
        public ActionResult Index()
        {
            return this.View();
        }

        [HttpGet]
        [RequirePermission(Permission.ViewRules)]
        public async Task<ActionResult> GetRuleProperties(string deviceId, string ruleId)
        {
            DeviceRule rule = await this.deviceRulesService.GetDeviceRuleAsync(deviceId, ruleId);
            EditDeviceRuleViewModel editModel = this.CreateEditModelFromDeviceRule(rule);
            return this.PartialView("_DeviceRuleProperties", editModel);
        }

        [HttpPost]
        [RequirePermission(Permission.EditRules)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateRuleProperties(EditDeviceRuleViewModel model)
        {
            string errorMessage = model.CheckForErrorMessage();
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                return this.Json(new { error = errorMessage });
            }

            DeviceRule rule = this.CreateDeviceRuleFromEditModel(model);
            TableStorageResponse<DeviceRule> response = await this.deviceRulesService.SaveDeviceRuleAsync(rule);

            return this.BuildRuleUpdateResponse(response);
        }

        [HttpGet]
        [RequirePermission(Permission.EditRules)]
        public async Task<ActionResult> GetNewRule(string deviceId)
        {
            DeviceRule rule = await this.deviceRulesService.GetNewRuleAsync(deviceId);
            return this.Json(rule);
        }

        [HttpPost]
        [RequirePermission(Permission.EditRules)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateRuleEnabledState(EditDeviceRuleViewModel ruleModel)
        {
            TableStorageResponse<DeviceRule> response = await this.deviceRulesService.UpdateDeviceRuleEnabledStateAsync(
                ruleModel.DeviceID,
                ruleModel.RuleId,
                ruleModel.EnabledState);

            return this.BuildRuleUpdateResponse(response);
        }

        [HttpDelete]
        [RequirePermission(Permission.DeleteRules)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteDeviceRule(string deviceId, string ruleId)
        {
            TableStorageResponse<DeviceRule> response = await this.deviceRulesService.DeleteDeviceRuleAsync(deviceId, ruleId);

            return this.BuildRuleUpdateResponse(response);
        }

        [RequirePermission(Permission.EditRules)]
        public async Task<ActionResult> EditRuleProperties(string deviceId, string ruleId)
        {
            EditDeviceRuleViewModel editModel;

            // empty ruleId implies that we are creating new
            if (string.IsNullOrWhiteSpace(ruleId))
            {
                bool canCreate = await this.deviceRulesService.CanNewRuleBeCreatedForDeviceAsync(deviceId);
                if (!canCreate)
                {
                    editModel = new EditDeviceRuleViewModel()
                    {
                        DeviceID = deviceId
                    };
                    return this.View("AllRulesAssigned", editModel);
                }
            }

            DeviceRule ruleModel = await this.deviceRulesService.GetDeviceRuleOrDefaultAsync(deviceId, ruleId);
            Dictionary<string, List<string>> availableFields = await this.deviceRulesService.GetAvailableFieldsForDeviceRuleAsync(ruleModel.DeviceID, ruleModel.RuleId);

            List<SelectListItem> availableDataFields = this.ConvertStringListToSelectList(availableFields["availableDataFields"]);
            List<SelectListItem> availableOperators = this.ConvertStringListToSelectList(availableFields["availableOperators"]);
            List<SelectListItem> availableRuleOutputs = this.ConvertStringListToSelectList(availableFields["availableRuleOutputs"]);

            editModel = this.CreateEditModelFromDeviceRule(ruleModel);
            editModel.AvailableDataFields = availableDataFields;
            editModel.AvailableOperators = availableOperators;
            editModel.AvailableRuleOutputs = availableRuleOutputs;

            return this.View("EditDeviceRuleProperties", editModel);
        }

        [RequirePermission(Permission.DeleteRules)]
        public async Task<ActionResult> RemoveRule(string deviceId, string ruleId)
        {
            DeviceRule ruleModel = await this.deviceRulesService.GetDeviceRuleOrDefaultAsync(deviceId, ruleId);
            EditDeviceRuleViewModel editModel = this.CreateEditModelFromDeviceRule(ruleModel);
            return this.View("RemoveDeviceRule", editModel);
        }

        private DeviceRule CreateDeviceRuleFromEditModel(EditDeviceRuleViewModel editModel)
        {
            DeviceRule rule = new DeviceRule(editModel.RuleId)
            {
                DataField = editModel.DataField,
                DeviceID = editModel.DeviceID,
                EnabledState = editModel.EnabledState,
                Etag = editModel.Etag,
                Operator = editModel.Operator,
                RuleOutput = editModel.RuleOutput
            };
            if (!string.IsNullOrWhiteSpace(editModel.Threshold))
            {
                rule.Threshold =
                    double.Parse(
                        editModel.Threshold,
                        NumberStyles.Float,
                        CultureInfo.CurrentCulture);
            }

            return rule;
        }

        private EditDeviceRuleViewModel CreateEditModelFromDeviceRule(DeviceRule rule)
        {
            EditDeviceRuleViewModel editModel = new EditDeviceRuleViewModel
            {
                RuleId = rule.RuleId,
                DataField = rule.DataField,
                DeviceID = rule.DeviceID,
                EnabledState = rule.EnabledState,
                Etag = rule.Etag,
                Operator = rule.Operator,
                RuleOutput = rule.RuleOutput
            };
            if (rule.Threshold != null)
            {
                editModel.Threshold = rule.Threshold.ToString();
            }

            return editModel;
        }

        private List<SelectListItem> ConvertStringListToSelectList(List<string> stringList)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            foreach (string item in stringList)
            {
                SelectListItem selectItem = new SelectListItem
                {
                    Value = item,
                    Text = item
                };
                result.Add(selectItem);
            }

            return result;
        }

        private JsonResult BuildRuleUpdateResponse(TableStorageResponse<DeviceRule> response)
        {
            switch (response.Status)
            {
                case TableStorageResponseStatus.Successful:
                    return this.Json(new
                    {
                        success = true
                    });
                case TableStorageResponseStatus.ConflictError:
                    return this.Json(new
                    {
                        error = Strings.TableDataSaveConflictErrorMessage,
                        entity = JsonConvert.SerializeObject(response.Entity)
                    });
                case TableStorageResponseStatus.DuplicateInsert:
                    return this.Json(new
                    {
                        error = Strings.RuleAlreadyAddedError,
                        entity = JsonConvert.SerializeObject(response.Entity)
                    });
                case TableStorageResponseStatus.NotFound:
                    return this.Json(new
                    {
                        error = Strings.UnableToRetrieveRuleFromService,
                        entity = JsonConvert.SerializeObject(response.Entity)
                    });
                default:
                    return this.Json(new
                    {
                        error = Strings.RuleUpdateError,
                        entity = JsonConvert.SerializeObject(response.Entity)
                    });
            }
        }
    }
}