// <copyright file="TelemetryApiController.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace Web.ApiControllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Common.Configuration;
    using Common.Helpers;
    using Common.Models;
    using IoTInfrastructure.Models;
    using IoTInfrastructure.Services;
    using Resources;
    using Web.Models;
    using Web.Security;

    [RoutePrefix("api/v1/telemetry")]
    public class TelemetryApiController : ApiControllerBase
    {
        private const double MaxDeviceSummaryAgeMinutes = 10.0;
        private const int DisplayedHistoryItems = 18;
        private const int MaxDevicesToDisplayOnDashboard = 200;

        private static readonly TimeSpan CautionAlertMaxDelta = TimeSpan.FromMinutes(91.0);
        private static readonly TimeSpan CriticalAlertMaxDelta = TimeSpan.FromMinutes(11.0);

        private readonly IAlertsService alertsService;
        private readonly IDeviceService deviceLogic;
        private readonly IDeviceTelemetryService deviceTelemetryService;
        private readonly IConfigurationProvider configProvider;

        public TelemetryApiController(
            IDeviceTelemetryService deviceTelemetryService,
            IAlertsService alertsService,
            IDeviceService deviceLogic,
            IConfigurationProvider configProvider)
        {
            EFGuard.NotNull(deviceTelemetryService, nameof(deviceTelemetryService));
            EFGuard.NotNull(alertsService, nameof(alertsService));
            EFGuard.NotNull(deviceLogic, nameof(deviceLogic));
            EFGuard.NotNull(configProvider, nameof(configProvider));

            this.deviceTelemetryService = deviceTelemetryService;
            this.alertsService = alertsService;
            this.deviceLogic = deviceLogic;
            this.configProvider = configProvider;
        }

        [HttpGet]
        [Route("dashboardDevicePane")]
        [ApiRequirePermission(Permission.ViewTelemetry)]
        public async Task<HttpResponseMessage> GetDashboardDevicePaneDataAsync(string deviceId)
        {
            this.ValidateArgumentNotNullOrWhitespace("deviceId", deviceId);

            DashboardDevicePaneDataViewModel result = new DashboardDevicePaneDataViewModel
            {
                DeviceId = deviceId
            };

            Func<Task<DashboardDevicePaneDataViewModel>> getTelemetry =
                async () =>
                {
                    DeviceModel device = await deviceLogic.GetDeviceAsync(deviceId);

                    IList<DeviceTelemetryFieldModel> telemetryFields;

                    try
                    {
                        telemetryFields = deviceLogic.ExtractTelemetry(device);
                        result.DeviceTelemetryFields = telemetryFields?.ToArray();
                    }
                    catch
                    {
                        var message =
                            new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)
                            {
                                Content = new StringContent(
                                    string.Format(Strings.InvalidDeviceTelemetryFormat, deviceId))
                            };
                        throw new HttpResponseException(message);
                    }

                    // Get Telemetry Summary
                    DeviceTelemetrySummaryModel summaryModel;

                    result.DeviceTelemetrySummaryModel = summaryModel =
                        await deviceTelemetryService.LoadLatestDeviceTelemetrySummaryAsync(
                            deviceId, DateTime.Now.AddMinutes(-MaxDeviceSummaryAgeMinutes));

                    if (summaryModel == null)
                    {
                        result.DeviceTelemetrySummaryModel =
                            new DeviceTelemetrySummaryModel();
                    }

                    // Get Telemetry History
                    DateTime minTime = DateTime.Now.AddMinutes(-MaxDeviceSummaryAgeMinutes);
                    var telemetryModels = await deviceTelemetryService.LoadLatestDeviceTelemetryAsync(deviceId, telemetryFields, minTime);

                    result.DeviceTelemetryModels = telemetryModels?.OrderBy(t => t.Timestamp).ToArray() ?? new DeviceTelemetryModel[0];

                    return result;
                };

            return await this.GetServiceResponseAsync<DashboardDevicePaneDataViewModel>(
                getTelemetry,
                false);
        }

        [HttpGet]
        [Route("list")]
        [ApiRequirePermission(Permission.ViewTelemetry)]
        public async Task<HttpResponseMessage> GetDeviceTelemetryAsync(
            string deviceId,
            DateTime minTime)
        {
            this.ValidateArgumentNotNullOrWhitespace("deviceId", deviceId);

            Func<Task<DeviceTelemetryModel[]>> getTelemetry =
                async () =>
                {
                    DeviceModel device = await deviceLogic.GetDeviceAsync(deviceId);

                    IList<DeviceTelemetryFieldModel> telemetryFields;

                    try
                    {
                        telemetryFields = deviceLogic.ExtractTelemetry(device);
                    }
                    catch
                    {
                        var message =
                            new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)
                            {
                                Content = new StringContent(
                                    string.Format(Strings.InvalidDeviceTelemetryFormat, deviceId))
                            };
                        throw new HttpResponseException(message);
                    }

                    IEnumerable<DeviceTelemetryModel> telemetryModels =
                        await deviceTelemetryService.LoadLatestDeviceTelemetryAsync(
                            deviceId,
                            telemetryFields,
                            minTime);

                    return telemetryModels?.OrderBy(t => t.Timestamp).ToArray() ?? new DeviceTelemetryModel[0];
                };

            return await this.GetServiceResponseAsync<DeviceTelemetryModel[]>(
                getTelemetry,
                false);
        }

        [HttpGet]
        [Route("summary")]
        [ApiRequirePermission(Permission.ViewTelemetry)]
        public async Task<HttpResponseMessage> GetDeviceTelemetrySummaryAsync(string deviceId)
        {
            this.ValidateArgumentNotNullOrWhitespace("deviceId", deviceId);

            Func<Task<DeviceTelemetrySummaryModel>> getTelemetrySummary =
                async () => await this.deviceTelemetryService.LoadLatestDeviceTelemetrySummaryAsync(
                    deviceId,
                    null);

            return await this.GetServiceResponseAsync<DeviceTelemetrySummaryModel>(
                getTelemetrySummary,
                false);
        }

        [HttpGet]
        [Route("alertHistory")]
        [ApiRequirePermission(Permission.ViewTelemetry)]
        public async Task<HttpResponseMessage> GetLatestAlertHistoryAsync()
        {
            Func<Task<AlertHistoryResultsViewModel>> loadHistoryItems =
                async () =>
                {
                    // Dates are stored internally as UTC and marked as such.  
                    // When parsed, they'll be made relative to the server's 
                    // time zone.  This is only in an issue on servers machines, 
                    // not set to GMT.
                    DateTime currentTime = DateTime.Now;

                    var historyItems = new List<AlertHistoryItemModel>();
                    var deviceModels = new List<AlertHistoryDeviceViewModel>();
                    var resultsModel = new AlertHistoryResultsViewModel();

                    IEnumerable<AlertHistoryItemModel> data =
                        await alertsService.LoadLatestAlertHistoryAsync(
                            currentTime.Subtract(CautionAlertMaxDelta),
                            DisplayedHistoryItems);

                    if (data != null)
                    {
                        historyItems.AddRange(data);

                        // get alert history
                        List<DeviceModel> devices = await this.LoadAllDevicesAsync();

                        if (devices != null)
                        {
                            DeviceListLocationsModel locationsModel = deviceLogic.ExtractLocationsData(devices);
                            if (locationsModel != null)
                            {
                                resultsModel.MaxLatitude = locationsModel.MaximumLatitude;
                                resultsModel.MaxLongitude = locationsModel.MaximumLongitude;
                                resultsModel.MinLatitude = locationsModel.MinimumLatitude;
                                resultsModel.MinLongitude = locationsModel.MinimumLongitude;

                                if (locationsModel.DeviceLocationList != null)
                                {
                                    Func<string, DateTime?> getStatusTime =
                                        deviceTelemetryService.ProduceGetLatestDeviceAlertTime(historyItems);

                                    foreach (DeviceLocationModel locationModel in locationsModel.DeviceLocationList)
                                    {
                                        if (string.IsNullOrWhiteSpace(locationModel?.DeviceId))
                                        {
                                            continue;
                                        }

                                        var deviceModel = new AlertHistoryDeviceViewModel()
                                        {
                                            DeviceId = locationModel.DeviceId,
                                            Latitude = locationModel.Latitude,
                                            Longitude = locationModel.Longitude
                                        };

                                        DateTime? lastStatusTime = getStatusTime(locationModel.DeviceId);
                                        if (lastStatusTime.HasValue)
                                        {
                                            TimeSpan deltaTime = currentTime - lastStatusTime.Value;

                                            if (deltaTime < CriticalAlertMaxDelta)
                                            {
                                                deviceModel.Status = AlertHistoryDeviceStatus.Critical;
                                            }
                                            else if (deltaTime < CautionAlertMaxDelta)
                                            {
                                                deviceModel.Status = AlertHistoryDeviceStatus.Caution;
                                            }
                                        }

                                        deviceModels.Add(deviceModel);
                                    }
                                }
                            }
                        }
                    }

                    resultsModel.Data = historyItems.Take(DisplayedHistoryItems).ToList();
                    resultsModel.Devices = deviceModels;
                    resultsModel.TotalAlertCount = historyItems.Count;
                    resultsModel.TotalFilteredCount = historyItems.Count;

                    return resultsModel;
                };

            return await this.GetServiceResponseAsync<AlertHistoryResultsViewModel>(loadHistoryItems, false);
        }

        [HttpGet]
        [Route("deviceLocationData")]
        [ApiRequirePermission(Permission.ViewTelemetry)]
        public async Task<HttpResponseMessage> GetDeviceLocationData()
        {
            return await this.GetServiceResponseAsync<DeviceListLocationsModel>(
                async () =>
                {
                    var query = new DeviceListQuery()
                    {
                        Skip = 0,
                        Take = MaxDevicesToDisplayOnDashboard,
                        SortColumn = "DeviceID"
                    };

                    DeviceListQueryResult queryResult = await deviceLogic.GetDevices(query);
                    DeviceListLocationsModel dataModel = deviceLogic.ExtractLocationsData(queryResult.Results);

                    return dataModel;
                },
                false);
        }

        [HttpGet]
        [Route("mapApiKey")]
        [ApiRequirePermission(Permission.ViewTelemetry)]
        public async Task<HttpResponseMessage> GetMapApiKey()
        {
            return await this.GetServiceResponseAsync<string>(
                async () =>
                {
                    string keySetting = await Task.Run(() =>
                    {
                        // Set key to empty if passed value 0 from arm template
                        string key = configProvider.GetConfigurationSettingValue("MapApiQueryKey");
                        return key.Equals("0") ? string.Empty : key;
                    });
                    return keySetting;
                },
                false);
        }

        private async Task<List<DeviceModel>> LoadAllDevicesAsync()
        {
            var query = new DeviceListQuery()
            {
                Skip = 0,
                Take = MaxDevicesToDisplayOnDashboard,
                SortColumn = "DeviceID"
            };

            var devices = new List<DeviceModel>();
            DeviceListQueryResult queryResult = await this.deviceLogic.GetDevices(query);

            if (queryResult?.Results != null)
            {
                devices.AddRange(queryResult.Results.Where(
                    devInfo => !string.IsNullOrWhiteSpace(devInfo.DeviceProperties.DeviceID)));
            }

            return devices;
        }
    }
}