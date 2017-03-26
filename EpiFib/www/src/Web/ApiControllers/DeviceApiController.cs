// <copyright file="DeviceApiController.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace Web.ApiControllers
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Common.Helpers;
    using Common.Models;
    using IoTInfrastructure.Models;
    using IoTInfrastructure.Services;
    using Newtonsoft.Json.Linq;
    using Web.Models.DataTables;
    using Web.Security;

    [Authorize]
    [RoutePrefix("api/v1/devices")]
    public class DeviceApiController : ApiControllerBase
    {
        private readonly IDeviceService deviceService;

        public DeviceApiController(IDeviceService deviceService)
        {
            EFGuard.NotNull(deviceService, nameof(deviceService));

            this.deviceService = deviceService;
        }

        [HttpPost]
        [Route("list")]
        [ApiRequirePermission(Permission.ViewDevices)]
        public async Task<HttpResponseMessage> GetDevices([FromBody]JObject requestData)
        {
            return await this.GetServiceResponseAsync<DataTablesResponse<DeviceModel>>(
                async () =>
                {
                    var dataTableRequest = requestData.ToObject<DataTablesRequest>();
                    var sortColumnIndex = dataTableRequest.SortColumns[0].ColumnIndex;

                    var listQuery = new DeviceListQuery()
                    {
                        SortOrder = dataTableRequest.SortColumns[0].SortOrder,
                        SortColumn = dataTableRequest.Columns[sortColumnIndex].Name,

                        SearchQuery = dataTableRequest.Search.Value,

                        Filters = dataTableRequest.Filters,

                        Skip = dataTableRequest.Start,
                        Take = dataTableRequest.Length
                    };

                    var queryResult = await this.deviceService.GetDevices(listQuery);

                    var dataTablesResponse = new DataTablesResponse<DeviceModel>
                    {
                        Draw = dataTableRequest.Draw,
                        RecordsTotal = queryResult.TotalDeviceCount,
                        RecordsFiltered = queryResult.TotalFilteredCount,
                        Data = queryResult.Results.ToArray()
                    };

                    return dataTablesResponse;
                }, 
                false);
        }

        // PUT: api/v1/devices/5/enabledstatus
        [HttpPut]
        [Route("{deviceId}/enabledstatus")]
        [ApiRequirePermission(Permission.DisableEnableDevices)]
        public async Task<HttpResponseMessage> UpdateDeviceEnabledStatus(string deviceId, [FromBody]JObject request)
        {
            bool isEnabled;

            this.ValidateArgumentNotNullOrWhitespace("deviceId", deviceId);

            if (request == null)
            {
                return this.GetNullRequestErrorResponse<bool>();
            }

            try
            {
                var property = request.Property("isEnabled");

                if (property == null)
                {
                    return this.GetFormatErrorResponse<bool>("isEnabled", "bool");
                }

                isEnabled = request.Value<bool>("isEnabled");
            }
            catch (Exception)
            {
                return this.GetFormatErrorResponse<bool>("isEnabled", "bool");
            }

            return await this.GetServiceResponseAsync(async () =>
            {
                await this.deviceService.UpdateDeviceEnabledStatusAsync(deviceId, isEnabled);
                return true;
            });
        }
    }
}