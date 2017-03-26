// <copyright file="DeviceTelemetryRepository.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace IoTInfrastructure.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.Configuration;
    using Common.Helpers;
    using Common.Models;
    using IoTInfrastructure.Models;
    using StrDict = System.Collections.Generic.IDictionary<string, string>;

    public class DeviceTelemetryRepository : IDeviceTelemetryRepository
    {
        private readonly string telemetryDataPrefix;
        private readonly string telemetrySummaryPrefix;
        private readonly IBlobStorageClient blobStorageManager;

        public DeviceTelemetryRepository(IConfigurationProvider configProvider, IBlobStorageClientFactory blobStorageClientFactory)
        {
            EFGuard.NotNull(configProvider, nameof(configProvider));
            EFGuard.NotNull(blobStorageClientFactory, nameof(blobStorageClientFactory));

            string telemetryContainerName = configProvider.GetConfigurationSettingValue("tel.TelemetryStoreContainerName");
            this.telemetryDataPrefix = configProvider.GetConfigurationSettingValue("tel.TelemetryDataPrefix");
            string telemetryStoreConnectionString = configProvider.GetConfigurationSettingValue("device.StorageConnectionString");
            this.telemetrySummaryPrefix = configProvider.GetConfigurationSettingValue("tel.TelemetrySummaryPrefix");
            this.blobStorageManager = blobStorageClientFactory.CreateClient(telemetryStoreConnectionString, telemetryContainerName);
        }

        public async Task<IEnumerable<DeviceTelemetryModel>> LoadLatestDeviceTelemetryAsync(
            string deviceId,
            IList<DeviceTelemetryFieldModel> telemetryFields, 
            DateTime minTime)
        {
            IEnumerable<DeviceTelemetryModel> result = new DeviceTelemetryModel[0];

            IBlobStorageReader telemetryBlobReader = await this.blobStorageManager.GetReader(
                this.telemetryDataPrefix, 
                minTime);
            foreach (BlobContents telemetryStream in telemetryBlobReader)
            {
                IEnumerable<DeviceTelemetryModel> blobModels;
                try
                {
                    blobModels = LoadBlobTelemetryModels(telemetryStream.Data, telemetryFields);
                }
                catch
                {
                    continue;
                }

                if (blobModels == null)
                {
                    break;
                }

                int preFilterCount = blobModels.Count();

                blobModels =
                    blobModels.Where(
                        t => t?.Timestamp != null && t.Timestamp.Value >= minTime);

                if (preFilterCount == 0)
                {
                    break;
                }

                result = result.Concat(blobModels);
            }

            if (!string.IsNullOrEmpty(deviceId))
            {
                result = result.Where(t => t.DeviceId == deviceId);
            }

            return result;
        }

        public async Task<DeviceTelemetrySummaryModel> LoadLatestDeviceTelemetrySummaryAsync(
            string deviceId,
            DateTime? minTime)
        {
            DeviceTelemetrySummaryModel summaryModel = null;
            IBlobStorageReader telemetryBlobReader = await this.blobStorageManager.GetReader(
                this.telemetrySummaryPrefix, 
                minTime);
            foreach (BlobContents telemetryStream in telemetryBlobReader)
            {
                IEnumerable<DeviceTelemetrySummaryModel> blobModels;
                try
                {
                    blobModels = LoadBlobTelemetrySummaryModels(telemetryStream.Data, telemetryStream.LastModifiedTime);
                }
                catch
                {
                    continue;
                }

                if (blobModels == null)
                {
                    break;
                }

                blobModels = blobModels.Where(t => t != null);

                if (!string.IsNullOrEmpty(deviceId))
                {
                    blobModels = blobModels.Where(t => t.DeviceId == deviceId);
                }

                summaryModel = blobModels.LastOrDefault();
                if (summaryModel != null)
                {
                    break;
                }
            }

            return summaryModel;
        }

        private static List<DeviceTelemetryModel> LoadBlobTelemetryModels(
            Stream stream,
            IList<DeviceTelemetryFieldModel> telemetryFields)
        {
            EFGuard.NotNull(stream, nameof(stream));

            List<DeviceTelemetryModel> models = new List<DeviceTelemetryModel>();

            TextReader reader = null;
            try
            {
                stream.Position = 0;
                reader = new StreamReader(stream);

                IEnumerable<StrDict> strdicts = ParsingHelper.ParseCsv(reader).ToDictionaries();
                foreach (StrDict strdict in strdicts)
                {
                    var model = new DeviceTelemetryModel();

                    string str;
                    if (strdict.TryGetValue("deviceid", out str))
                    {
                        model.DeviceId = str;
                    }

                    model.Timestamp = DateTime.Parse(
                        strdict["eventenqueuedutctime"],
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AllowWhiteSpaces);

                    IEnumerable<DeviceTelemetryFieldModel> fields;

                    if (telemetryFields != null && telemetryFields.Count > 0)
                    {
                        fields = telemetryFields;
                    }
                    else
                    {
                        List<string> reservedColumns = new List<string>
                        {
                            "DeviceId",
                            "EventEnqueuedUtcTime",
                            "EventProcessedUtcTime",
                            "IoTHub",
                            "PartitionId"
                        };

                        fields = strdict.Keys
                            .Where((key) => !reservedColumns.Contains(key))
                            .Select((name) => new DeviceTelemetryFieldModel
                            {
                                Name = name,
                                Type = "double"
                            });
                    }

                    foreach (var field in fields)
                    {
                        if (strdict.TryGetValue(field.Name, out str))
                        {
                            switch (field.Type.ToLowerInvariant())
                            {
                                case "int":
                                case "int16":
                                case "int32":
                                case "int64":
                                case "sbyte":
                                case "byte":
                                    int intValue;
                                    if (
                                        int.TryParse(
                                            str,
                                            NumberStyles.Integer,
                                            CultureInfo.InvariantCulture,
                                            out intValue) &&
                                        !model.Values.ContainsKey(field.Name))
                                    {
                                        model.Values.Add(field.Name, intValue);
                                    }

                                    break;

                                case "double":
                                case "decimal":
                                case "single":
                                    double dblValue;
                                    if (
                                        double.TryParse(
                                            str,
                                            NumberStyles.Float,
                                            CultureInfo.InvariantCulture,
                                            out dblValue) &&
                                        !model.Values.ContainsKey(field.Name))
                                    {
                                        model.Values.Add(field.Name, dblValue);
                                    }

                                    break;
                            }
                        }
                    }

                    models.Add(model);
                }
            }
            finally
            {
                IDisposable disp;

                if ((disp = stream) != null)
                {
                    disp.Dispose();
                }

                if ((disp = reader) != null)
                {
                    disp.Dispose();
                }
            }

            return models;
        }

        private static List<DeviceTelemetrySummaryModel> LoadBlobTelemetrySummaryModels(
            Stream stream,
            DateTime? lastModifiedTime)
        {
            EFGuard.NotNull(stream, nameof(stream));

            var models = new List<DeviceTelemetrySummaryModel>();

            TextReader reader = null;
            try
            {
                stream.Position = 0;
                reader = new StreamReader(stream);

                IEnumerable<StrDict> strdicts = ParsingHelper.ParseCsv(reader).ToDictionaries();
                foreach (StrDict strdict in strdicts)
                {
                    var model = new DeviceTelemetrySummaryModel();

                    string str;
                    if (strdict.TryGetValue("deviceid", out str))
                    {
                        model.DeviceId = str;
                    }

                    double number;
                    if (strdict.TryGetValue("averagehumidity", out str) &&
                        double.TryParse(
                            str,
                            NumberStyles.Float,
                            CultureInfo.InvariantCulture,
                            out number))
                    {
                        model.AverageHumidity = number;
                    }

                    if (strdict.TryGetValue("maxhumidity", out str) &&
                        double.TryParse(
                            str,
                            NumberStyles.Float,
                            CultureInfo.InvariantCulture,
                            out number))
                    {
                        model.MaximumHumidity = number;
                    }

                    if (strdict.TryGetValue("minimumhumidity", out str) &&
                        double.TryParse(
                            str,
                            NumberStyles.Float,
                            CultureInfo.InvariantCulture,
                            out number))
                    {
                        model.MinimumHumidity = number;
                    }

                    if (strdict.TryGetValue("timeframeminutes", out str) &&
                        double.TryParse(
                            str,
                            NumberStyles.Float,
                            CultureInfo.InvariantCulture,
                            out number))
                    {
                        model.TimeFrameMinutes = number;
                    }

                    model.Timestamp = lastModifiedTime;

                    models.Add(model);
                }
            }
            finally
            {
                IDisposable disp;
                if ((disp = stream) != null)
                {
                    disp.Dispose();
                }

                if ((disp = reader) != null)
                {
                    disp.Dispose();
                }
            }

            return models;
        }
    }
}
