// <copyright file="AlertsRepositoryTests.cs" company="The Reach Lab, LLC">
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

    public class AlertsRepositoryTests
    {
        private readonly Mock<IBlobStorageClient> blobStorageClientMock;
        private readonly AlertsRepository alertsRepository;

        public AlertsRepositoryTests()
        {
            IFixture fixture = new Fixture();
            fixture.Customize(new AutoConfiguredMoqCustomization());
            var configurationProviderMock = new Mock<IConfigurationProvider>();
            this.blobStorageClientMock = new Mock<IBlobStorageClient>();
            var blobStorageFactory = new BlobStorageClientFactory(this.blobStorageClientMock.Object);
            configurationProviderMock.Setup(
                x => x.GetConfigurationSettingValue(It.IsNotNull<string>()))
                .ReturnsUsingFixture(fixture);
            this.alertsRepository = new AlertsRepository(configurationProviderMock.Object, blobStorageFactory);
        }

        [Fact]
        public async void LoadLatestAlertHistoryAsyncTest()
        {
            var year = 2016;
            var month = 7;
            var date = 5;
            var value = "10.0";
            var minTime = new DateTime(year, month, date);

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                async () => await this.alertsRepository.LoadLatestAlertHistoryAsync(minTime, 0));

            var blobReader = new Mock<IBlobStorageReader>();
            string blobData = $"deviceid,reading,ruleoutput,time,{Environment.NewLine}Device123,{value},RuleOutput123,{minTime.ToString("o")}";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(blobData));
            var blobContents = new BlobContents { Data = stream, LastModifiedTime = DateTime.UtcNow };
            var blobContentIterable = new List<BlobContents>();
            blobContentIterable.Add(blobContents);

            blobReader.Setup(x => x.GetEnumerator()).Returns(blobContentIterable.GetEnumerator());

            this.blobStorageClientMock
                .Setup(x => x.GetReader(It.IsNotNull<string>(), It.IsAny<DateTime?>()))
                .ReturnsAsync(blobReader.Object);

            var alertsList = await this.alertsRepository.LoadLatestAlertHistoryAsync(minTime, 5);
            Assert.NotNull(alertsList);
            Assert.NotEmpty(alertsList);
            Assert.Equal(alertsList.First().Value, value);
            Assert.Equal(alertsList.First().Timestamp, minTime);
        }
    }
}
