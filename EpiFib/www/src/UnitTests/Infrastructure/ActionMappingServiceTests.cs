// <copyright file="ActionMappingServiceTests.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace UnitTests.Infrastructure
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using global::IoTInfrastructure.Models;
    using global::IoTInfrastructure.Repository;
    using global::IoTInfrastructure.Services;
    using Moq;
    using Ploeh.AutoFixture;
    using Xunit;

    public class ActionMappingServiceTests
    {
        private readonly Mock<IActionMappingRepository> actionMappingRepositoryMock;
        private readonly Mock<IDeviceRulesRepository> deviceRulesRepositoryMock;
        private readonly IFixture fixture;
        private readonly ActionMappingService actionMappingLogic;

        public ActionMappingServiceTests()
        {
            this.actionMappingRepositoryMock = new Mock<IActionMappingRepository>();
            this.deviceRulesRepositoryMock = new Mock<IDeviceRulesRepository>();
            this.fixture = new Fixture();
            this.actionMappingLogic = new ActionMappingService(
                this.actionMappingRepositoryMock.Object, 
                this.deviceRulesRepositoryMock.Object);
        }

        [Fact]
        public async void IsInitializationNeededAsyncTest()
        {
            this.actionMappingRepositoryMock.SetupSequence(x => x.GetAllMappingsAsync())
                .ReturnsAsync(new List<ActionMapping>())
                .ReturnsAsync(this.fixture.Create<List<ActionMapping>>());

            Assert.True(await this.actionMappingLogic.IsInitializationNeededAsync());
            Assert.False(await this.actionMappingLogic.IsInitializationNeededAsync());
        }

        [Fact]
        public async void InitializeDataIfNecessaryAsyncTest()
        {
            IList<ActionMapping> savedMappings = new List<ActionMapping>();
            this.actionMappingRepositoryMock.SetupSequence(x => x.GetAllMappingsAsync())
                .ReturnsAsync(new List<ActionMapping>());
            this.actionMappingRepositoryMock.Setup(x => x.SaveMappingAsync(It.IsNotNull<ActionMapping>()))
                .Callback<ActionMapping>(am => savedMappings.Add(am))
                .Returns(Task.FromResult(true));

            Assert.True(await this.actionMappingLogic.InitializeDataIfNecessaryAsync());
            Assert.Equal(2, savedMappings.Count);
            Assert.Equal("Send Message", savedMappings[0].ActionId);
            Assert.Equal("AlarmTemp", savedMappings[0].RuleOutput);
            Assert.Equal("Raise Alarm", savedMappings[1].ActionId);
            Assert.Equal("AlarmHumidity", savedMappings[1].RuleOutput);
        }

        [Fact]
        public async void GetAllMappingsAsyncTest()
        {
            var actionMappings = new List<ActionMapping>();
            this.fixture.Customize<ActionMapping>(ob => ob.With(x => x.RuleOutput, "RuleXXXOutput"));
            actionMappings.AddRange(this.fixture.CreateMany<ActionMapping>());
            this.fixture.Customize<ActionMapping>(ob => ob.With(x => x.RuleOutput, "RuleYYYOutput"));
            actionMappings.AddRange(this.fixture.CreateMany<ActionMapping>());
            var deviceRules = new List<DeviceRule>();
            this.fixture.Customize<DeviceRule>(ob => ob.With(x => x.RuleOutput, "RuleXXXOutput"));
            deviceRules.AddRange(this.fixture.CreateMany<DeviceRule>());
            var countXXXrules = deviceRules.Count;
            this.fixture.Customize<DeviceRule>(ob => ob.With(x => x.RuleOutput, "RuleYYYOutput"));
            deviceRules.AddRange(this.fixture.CreateMany<DeviceRule>());
            this.actionMappingRepositoryMock.Setup(x => x.GetAllMappingsAsync()).ReturnsAsync(actionMappings);
            this.deviceRulesRepositoryMock.Setup(x => x.GetAllRulesAsync()).ReturnsAsync(deviceRules);

            var ret = await this.actionMappingLogic.GetAllMappingsAsync();
            Assert.NotNull(ret);
            Assert.Equal(actionMappings.Count, ret.Count);
            Assert.Equal(actionMappings[0].ActionId, ret[0].ActionId);
            Assert.Equal(actionMappings[0].RuleOutput, ret[0].RuleOutput);
            Assert.Equal(countXXXrules, ret[0].NumberOfDevices);
        }

        [Fact]
        public async void GetActionIdFromRuleOutputAsyncTest()
        {
            var mappings = this.fixture.Create<List<ActionMapping>>();
            this.actionMappingRepositoryMock.Setup(x => x.GetAllMappingsAsync()).ReturnsAsync(mappings);

            Assert.Equal(string.Empty, await this.actionMappingLogic.GetActionIdFromRuleOutputAsync("RuleDoesNotExist"));
            Assert.Equal(mappings[0].ActionId, await this.actionMappingLogic.GetActionIdFromRuleOutputAsync(mappings[0].RuleOutput));
        }

        [Fact]
        public async void GetAvailableRuleOutputsAsyncTest()
        {
            var ret = await this.actionMappingLogic.GetAvailableRuleOutputsAsync();
            Assert.NotNull(ret);
            Assert.Equal(2, ret.Count);
            Assert.Equal("AlarmTemp", ret[0]);
            Assert.Equal("AlarmHumidity", ret[1]);
        }
    }
}
