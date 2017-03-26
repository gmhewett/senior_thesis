// <copyright file="AlertsRepository.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace IoTInfrastructure.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Dynamic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.Configuration;
    using Common.Helpers;
    using IoTInfrastructure.Models;

    public class AlertsRepository : IAlertsRepository
    {
        private const string DeviceIdColumnName = "deviceid";
        ////private const string READING_TYPE_COLUMN_NAME = "readingtype";
        private const string ReadingValueColumnName = "reading";
        ////private const string THRESHOLD_VALUE_COLUMN_NAME = "threshold";
        private const string RuleOutputColumnName = "ruleoutput";
        private const string TimeColumnName = "time";

        private readonly IBlobStorageClient blobStorageManager;
        private readonly string deviceAlertsDataPrefix;

        public AlertsRepository(IConfigurationProvider configProvider, IBlobStorageClientFactory blobStorageClientFactory)
        {
            EFGuard.NotNull(configProvider, nameof(configProvider));
            EFGuard.NotNull(blobStorageClientFactory, nameof(blobStorageClientFactory));

            string alertsContainerConnectionString = configProvider.GetConfigurationSettingValue("device.StorageConnectionString");
            string alertsStoreContainerName = configProvider.GetConfigurationSettingValue("alert.AlertsStoreContainerName");
            this.blobStorageManager = blobStorageClientFactory.CreateClient(alertsContainerConnectionString, alertsStoreContainerName);
            this.deviceAlertsDataPrefix = configProvider.GetConfigurationSettingValue("alert.DeviceAlertsDataPrefix");
        }

        public async Task<IEnumerable<AlertHistoryItemModel>> LoadLatestAlertHistoryAsync(DateTime minTime, int minResults)
        {
            if (minResults <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minResults), minResults, "minResults must be a positive integer.");
            }

            var filteredResult = new List<AlertHistoryItemModel>();
            var unfilteredResult = new List<AlertHistoryItemModel>();
            var alertBlobReader = await this.blobStorageManager.GetReader(this.deviceAlertsDataPrefix);
            foreach (var alertStream in alertBlobReader)
            {
                var segment = ProduceAlertHistoryItemsAsync(alertStream.Data);
                IEnumerable<AlertHistoryItemModel> filteredSegment = segment.Where(t => t?.Timestamp != null && (t.Timestamp.Value > minTime));

                var unfilteredCount = segment.Count;
                var filteredCount = filteredSegment.Count();

                unfilteredResult.AddRange(segment.OrderByDescending(t => t.Timestamp));
                filteredResult.AddRange(filteredSegment.OrderByDescending(t => t.Timestamp));

                // Anything filtered and min entries?
                if ((filteredCount != unfilteredCount) && (filteredResult.Count >= minResults))
                {
                    // already into items older than minTime
                    break;
                }

                // No more filtered entries and enough otherwise?
                if ((filteredCount == 0) && (unfilteredResult.Count >= minResults))
                {
                    // we are past minTime and we have enough unfiltered results
                    break;
                }
            }

            return filteredResult.Count >= minResults ? filteredResult : unfilteredResult.Take(minResults);
        }

        private static AlertHistoryItemModel ProduceAlertHistoryItem(ExpandoObject expandoObject)
        {
            Debug.Assert(expandoObject != null, "expandoObject is a null reference.");

            var deviceId = ReflectionHelper.GetNamedPropertyValue(
                        expandoObject,
                        DeviceIdColumnName,
                        true,
                        false) as string;

            var readingValue = ReflectionHelper.GetNamedPropertyValue(
                        expandoObject,
                        ReadingValueColumnName,
                        true,
                        false) as string;

            var ruleOutput = ReflectionHelper.GetNamedPropertyValue(
                        expandoObject,
                        RuleOutputColumnName,
                        true,
                        false) as string;

            var time = ReflectionHelper.GetNamedPropertyValue(
                        expandoObject,
                        TimeColumnName,
                        true,
                        false) as string;

            return BuildModelForItem(ruleOutput, deviceId, readingValue, time);
        }

        private static AlertHistoryItemModel BuildModelForItem(string ruleOutput, string deviceId, string value, string time)
        {
            double valDouble;
            DateTime timeAsDateTime;

            if (!string.IsNullOrWhiteSpace(value) &&
                !string.IsNullOrWhiteSpace(deviceId) &&
                double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out valDouble) &&
                DateTime.TryParse(time, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out timeAsDateTime))
            {
                return new AlertHistoryItemModel()
                {
                    RuleOutput = ruleOutput,
                    Value = value,
                    DeviceId = deviceId,
                    Timestamp = timeAsDateTime
                };
            }

            return null;
        }

        private static List<AlertHistoryItemModel> ProduceAlertHistoryItemsAsync(Stream stream)
        {
            Debug.Assert(stream != null, "stream is a null reference.");

            var models = new List<AlertHistoryItemModel>();
            TextReader reader = null;
            try
            {
                stream.Position = 0;
                reader = new StreamReader(stream);

                IEnumerable<ExpandoObject> expandos = ParsingHelper.ParseCsv(reader).ToExpandoObjects();
                foreach (ExpandoObject expando in expandos)
                {
                    AlertHistoryItemModel model = ProduceAlertHistoryItem(expando);

                    if (model != null)
                    {
                        models.Add(model);
                    }
                }
            }
            finally
            {
                IDisposable dispStream = stream;
                dispStream.Dispose();

                IDisposable dispReader = reader;
                dispReader?.Dispose();
            }

            return models;
        }
    }
}
