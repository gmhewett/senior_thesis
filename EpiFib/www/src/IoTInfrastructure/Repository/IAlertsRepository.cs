// <copyright file="IAlertsRepository.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace IoTInfrastructure.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using IoTInfrastructure.Models;

    public interface IAlertsRepository
    {
        Task<IEnumerable<AlertHistoryItemModel>> LoadLatestAlertHistoryAsync(DateTime minTime, int minResults);
    }
}
