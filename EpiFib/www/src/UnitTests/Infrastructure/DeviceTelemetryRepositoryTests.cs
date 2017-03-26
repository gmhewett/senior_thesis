// <copyright file="DeviceTelemetryRepositoryTests.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace UnitTests.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using global::Common.Configuration;
    using global::Common.Helpers;
    using global::Common.Models;
    using global::IoTInfrastructure.Repository;
    using Moq;
    using Ploeh.AutoFixture;
    using Ploeh.AutoFixture.AutoMoq;
    using Xunit;

    public class DeviceTelemetryRepositoryTests
    {
        private readonly Mock<IBlobStorageClient> blobStorageClientMock;
        private readonly DeviceTelemetryRepository deviceTelemetryRepository;

        public DeviceTelemetryRepositoryTests()
        {
            IFixture fixture = new Fixture();
            fixture.Customize(new AutoConfiguredMoqCustomization());
            var configurationProviderMock = new Mock<IConfigurationProvider>();
            this.blobStorageClientMock = new Mock<IBlobStorageClient>();
            var blobStorageFactory = new BlobStorageClientFactory(this.blobStorageClientMock.Object);
            configurationProviderMock.Setup(x => x.GetConfigurationSettingValue(It.IsNotNull<string>()))
                .ReturnsUsingFixture(fixture);
            this.deviceTelemetryRepository = new DeviceTelemetryRepository(
                configurationProviderMock.Object,
                blobStorageFactory);
        }

        [Fact]
        public async void LoadLatestDeviceTelemetryAsyncTest()
        {
            var year = 2016;
            var month = 7;
            var date = 5;
            var minTime = new DateTime(year, month, date);

            var blobReader = new Mock<IBlobStorageReader>();
            var blobData = "deviceid,temperature,humidity,externaltemperature,eventprocessedutctime,partitionid,eventenqueuedutctime,IoTHub" +
                Environment.NewLine +
                "test1,34.200411299709423,32.2233033525866,,2016-08-04T23:07:14.2549606Z,3," + minTime.ToString("o") + ",Record";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(blobData));
            var blobContents = new BlobContents { Data = stream, LastModifiedTime = DateTime.UtcNow };
            var blobContentIterable = new List<BlobContents> { blobContents };

            blobReader.Setup(x => x.GetEnumerator()).Returns(blobContentIterable.GetEnumerator());
            this.blobStorageClientMock.Setup(x => x.GetReader(It.IsNotNull<string>(), It.IsAny<DateTime?>()))
                .ReturnsAsync(blobReader.Object);
            var telemetryList = await this.deviceTelemetryRepository.LoadLatestDeviceTelemetryAsync("test1", null, minTime);
            Assert.NotNull(telemetryList);
            Assert.NotEmpty(telemetryList);
            Assert.Equal(telemetryList.First().DeviceId, "test1");
            Assert.Equal(telemetryList.First().Timestamp, minTime);
        }

        [Fact]
        public async void LoadLatestDeviceTelemetrySummaryAsyncTest()
        {
            var year = 2016;
            var month = 7;
            var date = 5;
            var minTime = new DateTime(year, month, date);

            var blobReader = new Mock<IBlobStorageReader>();
            var blobData = "deviceid,averagehumidity,minimumhumidity,maxhumidity,timeframeminutes" + Environment.NewLine +
                           "test2,37.806204872115607,37.806204872115607,37.806204872115607,5";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(blobData));
            var blobContents = new BlobContents { Data = stream, LastModifiedTime = DateTime.UtcNow };
            var blobContentIterable = new List<BlobContents> { blobContents };

            blobReader.Setup(x => x.GetEnumerator()).Returns(blobContentIterable.GetEnumerator());
            this.blobStorageClientMock.Setup(x => x.GetReader(It.IsNotNull<string>(), It.IsAny<DateTime?>()))
                .ReturnsAsync(blobReader.Object);
            var telemetrySummaryList =
                await this.deviceTelemetryRepository.LoadLatestDeviceTelemetrySummaryAsync("test2", minTime);
            Assert.NotNull(telemetrySummaryList);
            Assert.Equal(telemetrySummaryList.DeviceId, "test2");
            Assert.Equal(telemetrySummaryList.AverageHumidity, 37.806204872115607);
            Assert.Equal(telemetrySummaryList.MinimumHumidity, 37.806204872115607);
            Assert.Equal(telemetrySummaryList.MaximumHumidity, 37.806204872115607);
            Assert.Equal(telemetrySummaryList.TimeFrameMinutes, 5);
        }
    }
}
