// <copyright file="DeviceRulesServiceTests.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace UnitTests.Infrastructure
{
    using System.Collections.Generic;
    using global::Common.Models;
    using global::IoTInfrastructure.Models;
    using global::IoTInfrastructure.Repository;
    using global::IoTInfrastructure.Services;
    using Moq;
    using Ploeh.AutoFixture;
    using Xunit;

    public class DeviceRulesServiceTests
    {
        private readonly Mock<IDeviceRulesRepository> deviceRulesRepositoryMock;
        private readonly IDeviceRulesService deviceRulesService;
        private readonly Fixture fixture;

        public DeviceRulesServiceTests()
        {
            this.deviceRulesRepositoryMock = new Mock<IDeviceRulesRepository>();
            var actionMappingServiceMock = new Mock<IActionMappingService>();
            this.deviceRulesService = new DeviceRulesService(this.deviceRulesRepositoryMock.Object, actionMappingServiceMock.Object);
            this.fixture = new Fixture();
        }

        [Fact]
        public async void GetDeviceRuleOrDefaultAsyncTest()
        {
            var deviceId = this.fixture.Create<string>();
            var rules = this.fixture.Create<List<DeviceRule>>();
            rules.ForEach(x => x.DeviceID = deviceId);
            this.deviceRulesRepositoryMock.Setup(x => x.GetAllRulesForDeviceAsync(deviceId)).ReturnsAsync(rules);

            var ret = await this.deviceRulesService.GetDeviceRuleOrDefaultAsync(deviceId, rules[0].RuleId);
            Assert.Equal(rules[0], ret);

            ret = await this.deviceRulesService.GetDeviceRuleOrDefaultAsync(deviceId, "RuleNotPresent");
            Assert.NotNull(ret);
            Assert.Equal(deviceId, ret.DeviceID);
        }

        [Fact]
        public async void SaveDeviceRuleAsyncTest()
        {
            var deviceId = this.fixture.Create<string>();
            var rules = this.fixture.Create<List<DeviceRule>>();
            rules.ForEach(x => x.DeviceID = deviceId);
            this.deviceRulesRepositoryMock.Setup(x => x.GetAllRulesForDeviceAsync(deviceId)).ReturnsAsync(rules);
            this.deviceRulesRepositoryMock.Setup(x => x.SaveDeviceRuleAsync(It.IsNotNull<DeviceRule>()))
                .ReturnsAsync(new TableStorageResponse<DeviceRule>());

            var newRule = new DeviceRule();
            newRule.InitializeNewRule(deviceId);
            newRule.DataField = rules[0].DataField;
            var ret = await this.deviceRulesService.SaveDeviceRuleAsync(newRule);
            Assert.NotNull(ret.Entity);
            Assert.Equal(TableStorageResponseStatus.DuplicateInsert, ret.Status);

            newRule.InitializeNewRule(deviceId);
            newRule.DataField = "New data in DataField";
            ret = await this.deviceRulesService.SaveDeviceRuleAsync(newRule);
            Assert.NotNull(ret);
        }

        [Fact]
        public async void GetNewRuleAsyncTest()
        {
            var deviceId = this.fixture.Create<string>();
            var rule = await this.deviceRulesService.GetNewRuleAsync(deviceId);
            Assert.NotNull(rule);
            Assert.Equal(deviceId, rule.DeviceID);
        }

        [Fact]
        public async void UpdateDeviceRuleEnabledStateAsyncTest()
        {
            var rule = this.fixture.Create<DeviceRule>();
            rule.EnabledState = false;
            this.deviceRulesRepositoryMock.Setup(x => x.GetDeviceRuleAsync(rule.DeviceID, rule.RuleId)).ReturnsAsync(rule);
            bool prevState = false;
            this.deviceRulesRepositoryMock.Setup(
                x => x.SaveDeviceRuleAsync(It.Is<DeviceRule>(o => o.EnabledState != prevState)))
                .ReturnsAsync(new TableStorageResponse<DeviceRule>())
                .Verifiable();

            var res = await this.deviceRulesService.UpdateDeviceRuleEnabledStateAsync(rule.DeviceID, rule.RuleId, true);
            Assert.NotNull(res);
            this.deviceRulesRepositoryMock.Verify();

            prevState = rule.EnabledState;
            res = await this.deviceRulesService.UpdateDeviceRuleEnabledStateAsync(rule.DeviceID, rule.RuleId, false);
            Assert.NotNull(res);
            this.deviceRulesRepositoryMock.Verify();
        }
    }
}
