// <copyright file="VirtualDeviceTableStorageTests.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace UnitTests.Common
{
    using System.Collections.Generic;
    using System.Linq;
    using global::Common.Configuration;
    using global::Common.Helpers;
    using global::Common.Models;
    using global::Common.Repository;
    using Microsoft.WindowsAzure.Storage.Table;
    using Moq;
    using Ploeh.AutoFixture;
    using Ploeh.AutoFixture.AutoMoq;
    using Xunit;

    public class VirtualDeviceTableStorageTests
    {
        private readonly Mock<IAzureTableStorageClient> tableStorageClientMock;
        private readonly IVirtualDeviceStorage virtualDeviceStorage;
        private readonly IFixture fixture;

        public VirtualDeviceTableStorageTests()
        {
            this.fixture = new Fixture();
            var configProviderMock = new Mock<IConfigurationProvider>();
            configProviderMock.Setup(x => x.GetConfigurationSettingValue(It.IsNotNull<string>()))
                .ReturnsUsingFixture(this.fixture);
            this.tableStorageClientMock = new Mock<IAzureTableStorageClient>();
            var tableStorageClientFactory = new AzureTableStorageClientFactory(this.tableStorageClientMock.Object);
            this.virtualDeviceStorage = new VirtualDeviceTableStorage(configProviderMock.Object, tableStorageClientFactory);
        }

        [Fact]
        public async void GetDeviceListAsync()
        {
            var deviceEntities = this.fixture.Create<List<DeviceListEntity>>();
            this.tableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<DeviceListEntity>>()))
                .ReturnsAsync(deviceEntities);
            var ret = await this.virtualDeviceStorage.GetDeviceListAsync();
            Assert.NotNull(ret);
            Assert.Equal(deviceEntities.Count, ret.Count);
            Assert.Equal(deviceEntities[0].DeviceId, ret[0].DeviceId);
            Assert.Equal(deviceEntities[0].HostName, ret[0].HostName);
            Assert.Equal(deviceEntities[0].Key, ret[0].Key);
        }

        [Fact]
        public async void GetDeviceAsyncTest()
        {
            this.fixture.Customize<DeviceListEntity>(ob => ob.With(x => x.DeviceId, "DeviceXXXId"));
            var entities = this.fixture.CreateMany<DeviceListEntity>().ToList();
            this.tableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<DeviceListEntity>>()))
                .ReturnsAsync(entities);
            var ret = await this.virtualDeviceStorage.GetDeviceAsync("DeviceXXXId");
            Assert.NotNull(ret);
            Assert.Equal(entities[0].DeviceId, ret.DeviceId);
            Assert.Equal(entities[0].HostName, ret.HostName);
            Assert.Equal(entities[0].Key, ret.Key);
        }

        [Fact]
        public async void RemoveDeviceAsyncTest()
        {
            var entities = this.fixture.Create<List<DeviceListEntity>>();
            this.tableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<DeviceListEntity>>()))
                .ReturnsAsync(entities);
            this.tableStorageClientMock.Setup(x => x.ExecuteAsync(It.IsNotNull<TableOperation>()))
                .ReturnsAsync(new TableResult() { Result = entities[0] });
            Assert.True(await this.virtualDeviceStorage.RemoveDeviceAsync(entities[0].DeviceId));
        }

        [Fact]
        public async void AddOrUpdateDeviceAsyncTest()
        {
            var deviceConfig = this.fixture.Create<InitialDeviceConfig>();
            this.tableStorageClientMock.Setup(x => x.ExecuteAsync(It.IsNotNull<TableOperation>()))
                .ReturnsAsync(new TableResult());
            await this.virtualDeviceStorage.AddOrUpdateDeviceAsync(deviceConfig);
        }
    }
}
