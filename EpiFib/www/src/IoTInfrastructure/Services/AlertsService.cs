// <copyright file="AlertsService.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace IoTInfrastructure.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Common.Helpers;
    using IoTInfrastructure.Models;
    using IoTInfrastructure.Repository;

    public class AlertsService : IAlertsService
    {
        private readonly IAlertsRepository alertsRepository;

        public AlertsService(IAlertsRepository alertsRepository)
        {
            EFGuard.NotNull(alertsRepository, nameof(alertsRepository));

            this.alertsRepository = alertsRepository;
        }

        public async Task<IEnumerable<AlertHistoryItemModel>> LoadLatestAlertHistoryAsync(DateTime cutoffTime, int minResults)
        {
            if (minResults <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(minResults),
                    minResults,
                    "minResults must be a positive integer.");
            }

            return await this.alertsRepository.LoadLatestAlertHistoryAsync(cutoffTime, minResults);
        }
    }
}
