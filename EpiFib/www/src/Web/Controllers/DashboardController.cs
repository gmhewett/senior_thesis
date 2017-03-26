// <copyright file="DashboardController.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;
    using Common.Configuration;
    using Common.Exceptions;
    using Common.Helpers;
    using Common.Models;
    using IoTInfrastructure.Models;
    using IoTInfrastructure.Services;
    using Web.Models;
    using Web.Security;
    using StringPair = System.Collections.Generic.KeyValuePair<string, string>;

    [Authorize]
    public class DashboardController : Controller
    {
        private const int MaxDevicesToDisplayOnDashboard = 200;
        private readonly string cultureCookieName;

        private readonly IDeviceService deviceService;
        private readonly IConfigurationProvider configProvider;

        public DashboardController(
            IConfigurationProvider configProvider,
            IDeviceService deviceService)
        {
            EFGuard.NotNull(configProvider, nameof(configProvider));
            EFGuard.NotNull(deviceService, nameof(deviceService));

            this.cultureCookieName = configProvider.GetConfigurationSettingValue("web.CultureCookieName");
            this.deviceService = deviceService;
            this.configProvider = configProvider;
        }

        [RequirePermission(Permission.ViewTelemetry)]
        public async Task<ActionResult> Index()
        {
            var model = new DashboardViewModel();
            var filters = new List<IoTInfrastructure.Models.FilterInfo>
            {
                new IoTInfrastructure.Models.FilterInfo
                {
                    ColumnName = "status",
                    FilterType = FilterType.Status,
                    FilterValue = "Running"
                }
            };
            
            var query = new DeviceListQuery
            {
                Skip = 0,
                Take = MaxDevicesToDisplayOnDashboard,
                SortColumn = "DeviceID",
                Filters = filters
            };

            DeviceListQueryResult queryResult = await this.deviceService.GetDevices(query);

            if (queryResult?.Results != null)
            {
                foreach (DeviceModel devInfo in queryResult.Results)
                {
                    string deviceId;
                    try
                    {
                        deviceId = devInfo.DeviceProperties.DeviceID;
                    }
                    catch (DeviceRequiredPropertyNotFoundException)
                    {
                        continue;
                    }

                    model.DeviceIdsForDropdown.Add(new StringPair(deviceId, deviceId));
                }
            }

            // Set key to empty if passed value 0 from arm template
            ////string key = configProvider.GetConfigurationSettingValue("MapApiQueryKey");
            ////model.MapApiQueryKey = key.Equals("0") ? string.Empty : key;
            model.MapApiQueryKey = string.Empty;

            return this.View(model);
        }

        [HttpGet]
        [Route("culture/{cultureName}")]
        public ActionResult SetCulture(string cultureName)
        {
            // Save culture in a cookie
            HttpCookie cookie = this.Request.Cookies[this.cultureCookieName];

            if (cookie != null)
            {
                cookie.Value = cultureName; // update cookie value
            }
            else
            {
                cookie = new HttpCookie(this.cultureCookieName);
                cookie.Value = cultureName;
                cookie.Expires = DateTime.Now.AddYears(1);
            }

            Response.Cookies.Add(cookie);

            return this.RedirectToAction("Index");
        }
    }
}