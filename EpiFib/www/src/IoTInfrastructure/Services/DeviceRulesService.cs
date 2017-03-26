// <copyright file="DeviceRulesService.cs" company="The Reach Lab, LLC">
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
    using IoTInfrastructure.Repository;

    public class DeviceRulesService : IDeviceRulesService
    {
        private readonly IDeviceRulesRepository deviceRulesRepository;
        private readonly IActionMappingService actionMappingService;

        public DeviceRulesService(IDeviceRulesRepository deviceRulesRepository, IActionMappingService actionMappingService)
        {
            this.deviceRulesRepository = deviceRulesRepository;
            this.actionMappingService = actionMappingService;
        }
        
        public async Task<List<DeviceRule>> GetAllRulesAsync()
        {
            return await this.deviceRulesRepository.GetAllRulesAsync();
        }

        public async Task<DeviceRule> GetDeviceRuleOrDefaultAsync(string deviceId, string ruleId)
        {
            List<DeviceRule> rulesForDevice = await this.deviceRulesRepository.GetAllRulesForDeviceAsync(deviceId);
            foreach (DeviceRule rule in rulesForDevice)
            {
                if (rule.RuleId == ruleId)
                {
                    return rule;
                }
            }

            var createdRule = new DeviceRule();
            createdRule.InitializeNewRule(deviceId);
            return createdRule;
        }

        public async Task<DeviceRule> GetDeviceRuleAsync(string deviceId, string ruleId)
        {
            return await this.deviceRulesRepository.GetDeviceRuleAsync(deviceId, ruleId);
        }

        public async Task<TableStorageResponse<DeviceRule>> SaveDeviceRuleAsync(DeviceRule updatedRule)
        {
            List<DeviceRule> foundForDevice = await this.deviceRulesRepository.GetAllRulesForDeviceAsync(updatedRule.DeviceID);
            foreach (DeviceRule rule in foundForDevice)
            {
                if (rule.DataField == updatedRule.DataField && rule.RuleId != updatedRule.RuleId)
                {
                    var response = new TableStorageResponse<DeviceRule>
                    {
                        Entity = rule,
                        Status = TableStorageResponseStatus.DuplicateInsert
                    };

                    return response;
                }
            }

            return await this.deviceRulesRepository.SaveDeviceRuleAsync(updatedRule);
        }

        public async Task<DeviceRule> GetNewRuleAsync(string deviceId)
        {
            return await Task.Run(() =>
            {
                var rule = new DeviceRule();
                rule.InitializeNewRule(deviceId);

                return rule;
            });
        }

        public async Task<TableStorageResponse<DeviceRule>> UpdateDeviceRuleEnabledStateAsync(string deviceId, string ruleId, bool enabled)
        {
            DeviceRule found = await this.deviceRulesRepository.GetDeviceRuleAsync(deviceId, ruleId);
            if (found == null)
            {
                var response = new TableStorageResponse<DeviceRule>
                {
                    Entity = null,
                    Status = TableStorageResponseStatus.NotFound
                };

                return response;
            }

            found.EnabledState = enabled;

            return await this.deviceRulesRepository.SaveDeviceRuleAsync(found);
        }

        public async Task<Dictionary<string, List<string>>> GetAvailableFieldsForDeviceRuleAsync(string deviceId, string ruleId)
        {
            List<string> availableDataFields = DeviceRuleDataFields.GetListOfAvailableDataFields();
            List<string> operators = new List<string>() { ">" };
            List<string> ruleOutputs = await this.actionMappingService.GetAvailableRuleOutputsAsync();

            Dictionary<string, List<string>> result = new Dictionary<string, List<string>>
            {
                { "availableDataFields", availableDataFields },
                { "availableOperators", operators },
                { "availableRuleOutputs", ruleOutputs }
            };

            return result;
        }

        public async Task<bool> CanNewRuleBeCreatedForDeviceAsync(string deviceId)
        {
            List<DeviceRule> existingRules = await this.deviceRulesRepository.GetAllRulesForDeviceAsync(deviceId);
            List<string> availableDataFields = DeviceRuleDataFields.GetListOfAvailableDataFields();

            return existingRules.Count != availableDataFields.Count;
        }

        public async Task BootstrapDefaultRulesAsync(List<string> existingDeviceIds)
        {
            DeviceRule temperatureRule = await this.GetNewRuleAsync(existingDeviceIds[0]);
            temperatureRule.DataField = DeviceRuleDataFields.Temperature;
            temperatureRule.RuleOutput = "AlarmTemp";
            temperatureRule.Threshold = 38.0d;
            await this.SaveDeviceRuleAsync(temperatureRule);

            DeviceRule humidityRule = await this.GetNewRuleAsync(existingDeviceIds[0]);
            humidityRule.DataField = DeviceRuleDataFields.Humidity;
            humidityRule.RuleOutput = "AlarmHumidity";
            humidityRule.Threshold = 48.0d;
            await this.SaveDeviceRuleAsync(humidityRule);
        }

        public async Task<TableStorageResponse<DeviceRule>> DeleteDeviceRuleAsync(string deviceId, string ruleId)
        {
            DeviceRule found = await this.deviceRulesRepository.GetDeviceRuleAsync(deviceId, ruleId);
            if (found == null)
            {
                var response = new TableStorageResponse<DeviceRule>
                {
                    Entity = null,
                    Status = TableStorageResponseStatus.NotFound
                };

                return response;
            }

            return await this.deviceRulesRepository.DeleteDeviceRuleAsync(found);
        }

        public async Task<bool> RemoveAllRulesForDeviceAsync(string deviceId)
        {
            bool result = true;

            List<DeviceRule> deviceRules = await this.deviceRulesRepository.GetAllRulesForDeviceAsync(deviceId);
            foreach (DeviceRule rule in deviceRules)
            {
                TableStorageResponse<DeviceRule> response = await this.deviceRulesRepository.DeleteDeviceRuleAsync(rule);
                if (response.Status != TableStorageResponseStatus.Successful)
                {
                    result = false;
                }
            }

            return result;
        }
    }
}
