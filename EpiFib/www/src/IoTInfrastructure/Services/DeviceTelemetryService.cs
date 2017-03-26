// <copyright file="DeviceTelemetryService.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace IoTInfrastructure.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.Helpers;
    using IoTInfrastructure.Models;
    using IoTInfrastructure.Repository;

    public class DeviceTelemetryService : IDeviceTelemetryService
    {
        private readonly IDeviceTelemetryRepository deviceTelemetryRepository;

        public DeviceTelemetryService(IDeviceTelemetryRepository deviceTelemetryRepository)
        {
            EFGuard.NotNull(deviceTelemetryRepository, nameof(deviceTelemetryRepository));

            this.deviceTelemetryRepository = deviceTelemetryRepository;
        }

        public async Task<IEnumerable<DeviceTelemetryModel>> LoadLatestDeviceTelemetryAsync(string deviceId, IList<DeviceTelemetryFieldModel> telemetryFields, DateTime minTime)
        {
            return await this.deviceTelemetryRepository.LoadLatestDeviceTelemetryAsync(deviceId, telemetryFields, minTime);
        }

        public async Task<DeviceTelemetrySummaryModel> LoadLatestDeviceTelemetrySummaryAsync(string deviceId, DateTime? minTime)
        {
            return await this.deviceTelemetryRepository.LoadLatestDeviceTelemetrySummaryAsync(deviceId, minTime);
        }

        public Func<string, DateTime?> ProduceGetLatestDeviceAlertTime(IEnumerable<AlertHistoryItemModel> alertHistoryModels)
        {
            EFGuard.NotNull(alertHistoryModels, nameof(alertHistoryModels));

            Dictionary<string, DateTime> index = new Dictionary<string, DateTime>();

            alertHistoryModels = alertHistoryModels.Where(
                t => !string.IsNullOrEmpty(t?.DeviceId) && t.Timestamp.HasValue);

            foreach (AlertHistoryItemModel model in alertHistoryModels)
            {
                DateTime date;
                if (index.TryGetValue(model.DeviceId, out date) && (date >= model.Timestamp))
                {
                    continue;
                }

                if (model.Timestamp != null)
                {
                    index[model.DeviceId] = model.Timestamp.Value;
                }
            }

            return deviceId =>
            {
                DateTime lastAlert;

                if (index.TryGetValue(deviceId, out lastAlert))
                {
                    return lastAlert;
                }

                return null;
            };
        }
    }
}
