// <copyright file="IDeviceTelemetryRepository.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace IoTInfrastructure.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using IoTInfrastructure.Models;

    public interface IDeviceTelemetryRepository
    {
        Task<IEnumerable<DeviceTelemetryModel>> LoadLatestDeviceTelemetryAsync(
            string deviceId,
            IList<DeviceTelemetryFieldModel> telemetryFields,
            DateTime minTime);

        Task<DeviceTelemetrySummaryModel> LoadLatestDeviceTelemetrySummaryAsync(
            string deviceId,
            DateTime? minTime);
    }
}
