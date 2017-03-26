// <copyright file="DeviceActionsApiController.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace Web.ApiControllers
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;
    using IoTInfrastructure.Models;
    using IoTInfrastructure.Services;
    using Web.Models.DataTables;
    using Web.Security;

    [RoutePrefix("api/v1/actions")]
    public class DeviceActionsApiController : ApiControllerBase
    {
        private readonly IActionMappingService actionMappingService;

        public DeviceActionsApiController(IActionMappingService actionMappingService)
        {
            this.actionMappingService = actionMappingService;
        }

        [HttpGet]
        [Route("")]
        [ApiRequirePermission(Permission.ViewActions)]
        public async Task<HttpResponseMessage> GetDeviceActionsAsync()
        {
            return await this.GetServiceResponseAsync(async () => await this.actionMappingService.GetAllMappingsAsync());
        }

        // POST: api/v1/actions/list
        // This endpoint is used by the jQuery DataTables grid to get data (and accepts an unusual data format based on that grid)
        [HttpPost]
        [Route("list")]
        [ApiRequirePermission(Permission.ViewActions)]
        public async Task<HttpResponseMessage> GetDeviceActionsAsDataTablesResponseAsync()
        {
            return await this.GetServiceResponseAsync<DataTablesResponse<ActionMappingExtended>>(
                async () =>
                {
                    List<ActionMappingExtended> queryResult = await actionMappingService.GetAllMappingsAsync();

                    var dataTablesResponse = new DataTablesResponse<ActionMappingExtended>()
                    {
                        RecordsTotal = queryResult.Count,
                        RecordsFiltered = queryResult.Count,
                        Data = queryResult.ToArray()
                    };

                    return dataTablesResponse;
                }, 
                false);
        }

        [HttpPut]
        [Route("update")]
        [ApiRequirePermission(Permission.AssignAction)]
        public async Task<HttpResponseMessage> UpdateActionAsync(string ruleOutput, string actionId)
        {
            var actionMapping = new ActionMapping
            {
                RuleOutput = ruleOutput,
                ActionId = actionId
            };

            return await this.GetServiceResponseAsync(async () =>
            {
                await actionMappingService.SaveMappingAsync(actionMapping);
            });
        }

        [HttpGet]
        [Route("ruleoutputs/{ruleoutput}")]
        [ApiRequirePermission(Permission.ViewActions)]
        public async Task<HttpResponseMessage> GetActionIdFromRuleOutputAsync(string ruleOutput)
        {
            return await this.GetServiceResponseAsync(async () => await this.actionMappingService.GetActionIdFromRuleOutputAsync(ruleOutput));
        }

        [HttpGet]
        [Route("ruleoutputs")]
        [ApiRequirePermission(Permission.ViewActions)]
        public async Task<HttpResponseMessage> GetAvailableRuleOutputsAsync()
        {
            return await this.GetServiceResponseAsync(
                async () => await this.actionMappingService.GetAvailableRuleOutputsAsync());
        }
    }
}