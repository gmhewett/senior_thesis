// <copyright file="IAlertsService.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace IoTInfrastructure.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using IoTInfrastructure.Models;

    public interface IAlertsService
    {
        Task<IEnumerable<AlertHistoryItemModel>> LoadLatestAlertHistoryAsync(DateTime cutoffTime, int minResults);
    }
}
