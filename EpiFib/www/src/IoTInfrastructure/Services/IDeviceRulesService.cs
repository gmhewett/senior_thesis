// <copyright file="IDeviceRulesService.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace IoTInfrastructure.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Common.Models;
    using IoTInfrastructure.Models;

    public interface IDeviceRulesService
    {
        Task<List<DeviceRule>> GetAllRulesAsync();

        Task<DeviceRule> GetDeviceRuleOrDefaultAsync(string deviceId, string ruleId);

        Task<DeviceRule> GetDeviceRuleAsync(string deviceId, string ruleId);

        Task<TableStorageResponse<DeviceRule>> SaveDeviceRuleAsync(DeviceRule updatedRule);

        Task<DeviceRule> GetNewRuleAsync(string deviceId);

        Task<TableStorageResponse<DeviceRule>> UpdateDeviceRuleEnabledStateAsync(string deviceId, string ruleId, bool enabled);

        Task<Dictionary<string, List<string>>> GetAvailableFieldsForDeviceRuleAsync(string deviceId, string ruleId);

        Task<bool> CanNewRuleBeCreatedForDeviceAsync(string deviceId);

        Task BootstrapDefaultRulesAsync(List<string> existingDeviceIds);

        Task<TableStorageResponse<DeviceRule>> DeleteDeviceRuleAsync(string deviceId, string ruleId);

        Task<bool> RemoveAllRulesForDeviceAsync(string deviceId);
    }
}
