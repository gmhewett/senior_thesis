// <copyright file="DeviceRulesApiController.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace Web.ApiControllers
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Common.Models;
    using IoTInfrastructure.Models;
    using IoTInfrastructure.Services;
    using Web.Models.DataTables;
    using Web.Security;

    [RoutePrefix("api/v1/devicerules")]
    public class DeviceRulesApiController : ApiControllerBase
    {
        private readonly IDeviceRulesService deviceRulesService;

        public DeviceRulesApiController(IDeviceRulesService deviceRulesService)
        {
            this.deviceRulesService = deviceRulesService;
        }

        // GET: api/v1/devicerules
        [HttpGet]
        [Route("")]
        [ApiRequirePermission(Permission.ViewRules)]
        public async Task<HttpResponseMessage> GetDeviceRulesAsync()
        {
            return await this.GetServiceResponseAsync(async () => await this.deviceRulesService.GetAllRulesAsync());
        }

        // POST: api/v1/devicerules/list
        [HttpPost]
        [Route("list")]
        [ApiRequirePermission(Permission.ViewRules)]
        public async Task<HttpResponseMessage> GetDeviceRulesAsDataTablesResponseAsync()
        {
            return await this.GetServiceResponseAsync<DataTablesResponse<DeviceRule>>(
                async () =>
                {
                    var queryResult = await this.deviceRulesService.GetAllRulesAsync();

                    var dataTablesResponse = new DataTablesResponse<DeviceRule>()
                    {
                        RecordsTotal = queryResult.Count,
                        RecordsFiltered = queryResult.Count,
                        Data = queryResult.ToArray()
                    };

                    return dataTablesResponse;
                }, 
                false);
        }

        // GET: api/v1/devicerules/{id}/{ruleId}
        [HttpGet]
        [Route("{deviceId}/{ruleId}")]
        [ApiRequirePermission(Permission.ViewRules)]
        public async Task<HttpResponseMessage> GetDeviceRuleOrDefaultAsync(string deviceId, string ruleId)
        {
            return await this.GetServiceResponseAsync(async () => await this.deviceRulesService.GetDeviceRuleOrDefaultAsync(deviceId, ruleId));
        }

        // GET: api/v1/devicerules/{id}/{ruleId}/availableFields
        [HttpGet]
        [Route("{deviceId}/{ruleId}/availableFields")]
        [ApiRequirePermission(Permission.ViewRules)]
        public async Task<HttpResponseMessage> GetAvailableFieldsForDeviceRuleAsync(string deviceId, string ruleId)
        {
            return await this.GetServiceResponseAsync(async () => await this.deviceRulesService.GetAvailableFieldsForDeviceRuleAsync(deviceId, ruleId));
        }

        // POST: api/v1/devicerules
        [HttpPost]
        [Route("")]
        [ApiRequirePermission(Permission.EditRules)]
        public async Task<HttpResponseMessage> SaveDeviceRuleAsync(DeviceRule updatedRule)
        {
            return await this.GetServiceResponseAsync<TableStorageResponse<DeviceRule>>(
                async () => await this.deviceRulesService.SaveDeviceRuleAsync(updatedRule));
        }

        // GET: api/v1/devicerules/{id}
        [HttpGet]
        [Route("{deviceId}")]
        [ApiRequirePermission(Permission.EditRules)]
        public async Task<HttpResponseMessage> GetNewRuleAsync(string deviceId)
        {
            return await this.GetServiceResponseAsync(async () => await this.deviceRulesService.GetNewRuleAsync(deviceId));
        }

        // PUT: api/v1/devicerules/2345/123/true
        [HttpPut]
        [Route("{deviceId}/{ruleId}/{enabled}")]
        [ApiRequirePermission(Permission.EditRules)]
        public async Task<HttpResponseMessage> UpdateRuleEnabledStateAsync(string deviceId, string ruleId, bool enabled)
        {
            return await this.GetServiceResponseAsync<TableStorageResponse<DeviceRule>>(
                async () => await this.deviceRulesService.UpdateDeviceRuleEnabledStateAsync(deviceId, ruleId, enabled));
        }

        // Delete: api/v1/devicerules/2345/123
        [HttpDelete]
        [Route("{deviceId}/{ruleId}")]
        [ApiRequirePermission(Permission.DeleteRules)]
        public async Task<HttpResponseMessage> DeleteRuleAsync(string deviceId, string ruleId)
        {
            return await this.GetServiceResponseAsync<TableStorageResponse<DeviceRule>>(
                async () => await this.deviceRulesService.DeleteDeviceRuleAsync(deviceId, ruleId));
        }
    }
}