// <copyright file="ActionMappingService.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace IoTInfrastructure.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.Helpers;
    using IoTInfrastructure.Models;
    using IoTInfrastructure.Repository;

    public class ActionMappingService : IActionMappingService
    {
        private const string RuleOutputAlarmTemp = "AlarmTemp";
        private const string RuleOutputAlarmHumidity = "AlarmHumidity";

        private readonly IActionMappingRepository actionMappingRepository;
        private readonly IDeviceRulesRepository rulesRepository;

        private readonly List<string> availableRuleOutputs = new List<string>()
        {
            RuleOutputAlarmTemp,
            RuleOutputAlarmHumidity
        };

        public ActionMappingService(IActionMappingRepository actionMappingRepository, IDeviceRulesRepository rulesRepository)
        {
            EFGuard.NotNull(actionMappingRepository, nameof(actionMappingRepository));
            EFGuard.NotNull(rulesRepository, nameof(rulesRepository));

            this.actionMappingRepository = actionMappingRepository;
            this.rulesRepository = rulesRepository;
        }

        public async Task<bool> IsInitializationNeededAsync()
        {
            var existingMappings = await this.actionMappingRepository.GetAllMappingsAsync();

            return existingMappings.Count <= 0;
        }

        public async Task<bool> InitializeDataIfNecessaryAsync()
        {
            var existingMappings = await this.actionMappingRepository.GetAllMappingsAsync();

            if (existingMappings.Count > 0)
            {
                return false;
            }

            var am1 = new ActionMapping
            {
                RuleOutput = RuleOutputAlarmTemp,
                ActionId = "Send Message"
            };

            await this.actionMappingRepository.SaveMappingAsync(am1);

            var am2 = new ActionMapping
            {
                RuleOutput = RuleOutputAlarmHumidity,
                ActionId = "Raise Alarm"
            };

            await this.actionMappingRepository.SaveMappingAsync(am2);

            return true;
        }

        public async Task<List<ActionMappingExtended>> GetAllMappingsAsync()
        {
            var rawMappingsTask = this.actionMappingRepository.GetAllMappingsAsync();
            var rulesTask = this.rulesRepository.GetAllRulesAsync();

            List<ActionMapping> mappings = await rawMappingsTask;
            List<DeviceRule> rules = await rulesTask;

            return mappings.Select(mapping => new ActionMappingExtended
            {
                RuleOutput = mapping.RuleOutput,
                ActionId = mapping.ActionId,
                NumberOfDevices = rules.Count(r => r.RuleOutput == mapping.RuleOutput)
            }).ToList();
        }

        public async Task<string> GetActionIdFromRuleOutputAsync(string ruleOutput)
        {
            var mappings = await this.actionMappingRepository.GetAllMappingsAsync();

            var correctMapping = mappings.SingleOrDefault(m => m.RuleOutput == ruleOutput);

            return correctMapping == null ? string.Empty : correctMapping.ActionId;
        }

        public async Task SaveMappingAsync(ActionMapping action)
        {
            await this.actionMappingRepository.SaveMappingAsync(action);
        }

        public async Task<List<string>> GetAvailableRuleOutputsAsync()
        {
            return await Task.Run(() => this.availableRuleOutputs);
        }
    }
}
