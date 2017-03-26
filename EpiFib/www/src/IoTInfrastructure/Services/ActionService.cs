// <copyright file="ActionService.cs" company="The Reach Lab, LLC">
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
    using IoTInfrastructure.Repository;

    public class ActionService : IActionService
    {
        private readonly IActionRepository actionRepository;

        public ActionService(IActionRepository actionRepository)
        {
            this.actionRepository = actionRepository;
        }

        public async Task<List<string>> GetAllActionIdsAsync()
        {
            return await this.actionRepository.GetAllActionIdsAsync();
        }

        public async Task<bool> ExecuteLogicAppAsync(string actionId, string deviceId, string measurementName, double measuredValue)
        {
            EFGuard.StringNotNull(actionId, nameof(actionId));
            EFGuard.StringNotNull(deviceId, nameof(deviceId));
            
            var validActionIds = await this.GetAllActionIdsAsync();
            if (!validActionIds.Contains(actionId))
            {
                throw new ArgumentException("actionId must be a valid ActionId value");
            }

            return await this.actionRepository.ExecuteLogicAppAsync(actionId, deviceId, measurementName, measuredValue);
        }
    }
}
