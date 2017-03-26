// <copyright file="DeviceTelemetryServiceTests.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace UnitTests.Infrastructure
{
    using System.Collections.Generic;
    using global::IoTInfrastructure.Models;
    using global::IoTInfrastructure.Repository;
    using global::IoTInfrastructure.Services;
    using Moq;
    using Ploeh.AutoFixture;
    using Xunit;

    public class DeviceTelemetryServiceTests
    {
        private readonly DeviceTelemetryService deviceTelemetryService;
        private readonly Fixture fixture;

        public DeviceTelemetryServiceTests()
        {
            var deviceTelemetryRepositoryMock = new Mock<IDeviceTelemetryRepository>();
            this.deviceTelemetryService = new DeviceTelemetryService(deviceTelemetryRepositoryMock.Object);
            this.fixture = new Fixture();
        }

        [Fact]
        public void ProduceGetLatestDeviceAlertTimeTest()
        {
            var history = this.fixture.Create<List<AlertHistoryItemModel>>();
            var getAlertTime = this.deviceTelemetryService.ProduceGetLatestDeviceAlertTime(history);

            Assert.Equal(history[0].Timestamp, getAlertTime(history[0].DeviceId));
        }
    }
}
