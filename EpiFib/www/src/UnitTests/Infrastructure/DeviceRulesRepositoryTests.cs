// <copyright file="DeviceRulesRepositoryTests.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace UnitTests.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using global::Common.Configuration;
    using global::Common.Helpers;
    using global::Common.Models;
    using global::IoTInfrastructure.Models;
    using global::IoTInfrastructure.Repository;
    using Microsoft.WindowsAzure.Storage.Table;
    using Moq;
    using Ploeh.AutoFixture;
    using Ploeh.AutoFixture.AutoMoq;
    using Xunit;

    public class DeviceRulesRepositoryTests
    {
        private readonly Mock<IBlobStorageClient> blobClientMock;
        private readonly Mock<IAzureTableStorageClient> tableStorageClientMock;
        private readonly DeviceRulesRepository deviceRulesRepository;
        private readonly IFixture fixture;

        public DeviceRulesRepositoryTests()
        {
            this.fixture = new Fixture();
            var configProviderMock = new Mock<IConfigurationProvider>();
            this.tableStorageClientMock = new Mock<IAzureTableStorageClient>();
            this.blobClientMock = new Mock<IBlobStorageClient>();
            configProviderMock.Setup(x => x.GetConfigurationSettingValue(It.IsNotNull<string>()))
                .ReturnsUsingFixture(this.fixture);
            var tableStorageClientFactory = new AzureTableStorageClientFactory(this.tableStorageClientMock.Object);
            var blobClientFactory = new BlobStorageClientFactory(this.blobClientMock.Object);
            this.deviceRulesRepository = new DeviceRulesRepository(
                configProviderMock.Object, 
                tableStorageClientFactory,
                blobClientFactory);
        }

        [Fact]
        public async void GetAllRulesAsyncTest()
        {
            var ruleEntities = this.fixture.Create<List<DeviceRuleTableEntity>>();
            this.tableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<DeviceRuleTableEntity>>()))
                .ReturnsAsync(ruleEntities);
            var deviceRules = await this.deviceRulesRepository.GetAllRulesAsync();
            Assert.NotNull(deviceRules);
            Assert.Equal(ruleEntities.Count, deviceRules.Count);
            Assert.Equal(ruleEntities[0].DataField, deviceRules[0].DataField);
            Assert.Equal(ruleEntities[0].DeviceId, deviceRules[0].DeviceID);
            Assert.Equal(ruleEntities[0].Threshold, deviceRules[0].Threshold);
            Assert.Equal(">", deviceRules[0].Operator);
            Assert.Equal(ruleEntities[0].RuleOutput, deviceRules[0].RuleOutput);
            Assert.Equal(ruleEntities[0].ETag, deviceRules[0].Etag);
        }

        [Fact]
        public async void GetDeviceRuleAsyncTest()
        {
            var ruleEntity = this.fixture.Create<DeviceRuleTableEntity>();
            this.tableStorageClientMock.Setup(x => x.Execute(It.IsNotNull<TableOperation>()))
                .Returns(new TableResult { Result = ruleEntity });
            var ret = await this.deviceRulesRepository.GetDeviceRuleAsync(ruleEntity.DeviceId, ruleEntity.RuleId);
            Assert.NotNull(ret);
            Assert.Equal(ruleEntity.DeviceId, ret.DeviceID);
            Assert.Equal(ruleEntity.DataField, ret.DataField);
            Assert.Equal(ruleEntity.Threshold, ret.Threshold);
            Assert.Equal(">", ret.Operator);
            Assert.Equal(ruleEntity.RuleOutput, ret.RuleOutput);
            Assert.Equal(ruleEntity.ETag, ret.Etag);
        }

        [Fact]
        public async void GetAllRulesForDeviceAsyncTest()
        {
            var ruleEntities = this.fixture.Create<List<DeviceRuleTableEntity>>();
            ruleEntities.ForEach(x => x.DeviceId = "DeviceXXXId");
            this.tableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<DeviceRuleTableEntity>>()))
                .ReturnsAsync(ruleEntities);
            var deviceRules = await this.deviceRulesRepository.GetAllRulesForDeviceAsync("DeviceXXXId");
            Assert.NotNull(deviceRules);
            Assert.Equal(ruleEntities.Count, deviceRules.Count);
            Assert.Equal(ruleEntities[0].DataField, deviceRules[0].DataField);
            Assert.Equal(ruleEntities[0].DeviceId, deviceRules[0].DeviceID);
            Assert.Equal(ruleEntities[0].Threshold, deviceRules[0].Threshold);
            Assert.Equal(">", deviceRules[0].Operator);
            Assert.Equal(ruleEntities[0].RuleOutput, deviceRules[0].RuleOutput);
            Assert.Equal(ruleEntities[0].ETag, deviceRules[0].Etag);
        }

        [Fact]
        public async void SaveDeviceRuleAsyncTest()
        {
            var newRule = this.fixture.Create<DeviceRule>();
            DeviceRuleTableEntity tableEntity = null;
            var resp = new TableStorageResponse<DeviceRule>
            {
                Entity = newRule,
                Status = TableStorageResponseStatus.Successful
            };
            this.tableStorageClientMock.Setup(
                x =>
                    x.DoTableInsertOrReplaceAsync(
                        It.IsNotNull<DeviceRuleTableEntity>(),
                        It.IsNotNull<Func<DeviceRuleTableEntity, DeviceRule>>()))
                .Callback<DeviceRuleTableEntity, Func<DeviceRuleTableEntity, DeviceRule>>(
                    (entity, func) => tableEntity = entity)
                .ReturnsAsync(resp);
            var tableEntities = this.fixture.Create<List<DeviceRuleTableEntity>>();
            this.tableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<DeviceRuleTableEntity>>()))
                .ReturnsAsync(tableEntities);
            string blobEntititesStr = null;
            this.blobClientMock.Setup(x => x.UploadTextAsync(It.IsNotNull<string>(), It.IsNotNull<string>()))
                .Callback<string, string>((name, blob) => blobEntititesStr = blob)
                .Returns(Task.FromResult(true));
            var ret = await this.deviceRulesRepository.SaveDeviceRuleAsync(newRule);
            Assert.NotNull(ret);
            Assert.Equal(resp, ret);
            Assert.NotNull(tableEntity);
            Assert.Equal(newRule.DeviceID, tableEntity.DeviceId);
            Assert.NotNull(blobEntititesStr);
        }

        [Fact]
        public async void DeleteDeviceRuleAsyncTest()
        {
            var newRule = this.fixture.Create<DeviceRule>();
            DeviceRuleTableEntity tableEntity = null;
            var resp = new TableStorageResponse<DeviceRule>
            {
                Entity = newRule,
                Status = TableStorageResponseStatus.Successful
            };
            this.tableStorageClientMock.Setup(
                x =>
                    x.DoDeleteAsync(
                        It.IsNotNull<DeviceRuleTableEntity>(),
                        It.IsNotNull<Func<DeviceRuleTableEntity, DeviceRule>>()))
                .Callback<DeviceRuleTableEntity, Func<DeviceRuleTableEntity, DeviceRule>>(
                    (entity, func) => tableEntity = entity)
                .ReturnsAsync(resp);
            var tableEntities = this.fixture.Create<List<DeviceRuleTableEntity>>();
            this.tableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<DeviceRuleTableEntity>>()))
                .ReturnsAsync(tableEntities);
            string blobEntititesStr = null;
            this.blobClientMock.Setup(x => x.UploadTextAsync(It.IsNotNull<string>(), It.IsNotNull<string>()))
                .Callback<string, string>((name, blob) => blobEntititesStr = blob)
                .Returns(Task.FromResult(true));
            var ret = await this.deviceRulesRepository.DeleteDeviceRuleAsync(newRule);
            Assert.NotNull(ret);
            Assert.Equal(resp, ret);
            Assert.NotNull(tableEntity);
            Assert.Equal(newRule.DeviceID, tableEntity.DeviceId);
            Assert.NotNull(blobEntititesStr);
        }
    }
}
