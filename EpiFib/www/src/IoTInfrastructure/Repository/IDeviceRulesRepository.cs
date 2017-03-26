// <copyright file="IDeviceRulesRepository.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace IoTInfrastructure.Repository
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Common.Models;
    using IoTInfrastructure.Models;

    public interface IDeviceRulesRepository
    {
        Task<List<DeviceRule>> GetAllRulesAsync();

        Task<DeviceRule> GetDeviceRuleAsync(string deviceId, string ruleId);

        Task<List<DeviceRule>> GetAllRulesForDeviceAsync(string deviceId);

        Task<TableStorageResponse<DeviceRule>> SaveDeviceRuleAsync(DeviceRule updatedRule);

        Task<TableStorageResponse<DeviceRule>> DeleteDeviceRuleAsync(DeviceRule ruleToDelete);
    }
}
