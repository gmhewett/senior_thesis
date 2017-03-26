// <copyright file="ActionMappingRepositoryTests.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace UnitTests.Infrastructure
{
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using global::Common.Configuration;
    using global::Common.Helpers;
    using global::IoTInfrastructure.Models;
    using global::IoTInfrastructure.Repository;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Moq;
    using Newtonsoft.Json;
    using Ploeh.AutoFixture;
    using Ploeh.AutoFixture.AutoMoq;
    using Xunit;

    public class ActionMappingRepositoryTests
    {
        private readonly Mock<IBlobStorageClient> blobClientMock;
        private readonly ActionMappingRepository actionMappingRepository;
        private readonly Fixture fixture;

        public ActionMappingRepositoryTests()
        {
            this.fixture = new Fixture();
            this.fixture.Customize(new AutoConfiguredMoqCustomization());
            this.blobClientMock = new Mock<IBlobStorageClient>();
            var configurationProvicerMock = new Mock<IConfigurationProvider>();
            configurationProvicerMock.Setup(x => x.GetConfigurationSettingValue(It.IsNotNull<string>()))
                .ReturnsUsingFixture(this.fixture);
            var blobStorageFactory = new BlobStorageClientFactory(this.blobClientMock.Object);
            this.actionMappingRepository = new ActionMappingRepository(configurationProvicerMock.Object, blobStorageFactory);
        }

        [Fact]
        public async void GetAllMappingsAsyncTest()
        {
            var actionMappings = this.fixture.Create<List<ActionMapping>>();
            var actionMappingsString = JsonConvert.SerializeObject(actionMappings);
            var actionMappingBlobData = Encoding.UTF8.GetBytes(actionMappingsString);
            this.blobClientMock.Setup(x => x.GetBlobData(It.IsNotNull<string>())).ReturnsAsync(actionMappingBlobData);
            this.blobClientMock.Setup(x => x.GetBlobEtag(It.IsNotNull<string>())).ReturnsUsingFixture(this.fixture);
            var ret = await this.actionMappingRepository.GetAllMappingsAsync();
            Assert.NotNull(ret);
            Assert.Equal(actionMappingsString, JsonConvert.SerializeObject(ret));
        }

        [Fact]
        public async void SaveMappingAsyncTest()
        {
            var actionMappings = this.fixture.Create<List<ActionMapping>>();
            var actionMappingsString = JsonConvert.SerializeObject(actionMappings);
            var actionMappingBlobData = Encoding.UTF8.GetBytes(actionMappingsString);
            this.blobClientMock.Setup(x => x.GetBlobData(It.IsNotNull<string>())).ReturnsAsync(actionMappingBlobData);
            this.blobClientMock.Setup(x => x.GetBlobEtag(It.IsNotNull<string>())).ReturnsUsingFixture(this.fixture);

            string saveBuf = null;
            var newActionMapping = new ActionMapping
            {
                RuleOutput = "ruleXXXoutput",
                ActionId = "actionXXXid"
            };

            // New mapping
            actionMappings.Add(newActionMapping);
            actionMappingsString = JsonConvert.SerializeObject(actionMappings);
            actionMappings.Remove(newActionMapping);
            this.blobClientMock.Setup(
                x => x.UploadFromByteArrayAsync(
                        It.IsNotNull<string>(),
                        It.IsNotNull<byte[]>(), 
                        It.IsNotNull<int>(),
                        It.IsNotNull<int>(),
                        It.IsNotNull<AccessCondition>(), 
                        It.IsAny<BlobRequestOptions>(),
                        It.IsAny<OperationContext>()))
                .Callback<string, byte[], int, int, AccessCondition, BlobRequestOptions, OperationContext>(
                    (a, b, c, d, e, f, g) => saveBuf = Encoding.UTF8.GetString(b))
                .Returns(Task.FromResult(true));
            await this.actionMappingRepository.SaveMappingAsync(newActionMapping);
            Assert.NotNull(saveBuf);
            Assert.Equal(actionMappingsString, saveBuf);

            // Existing mapping
            actionMappingBlobData = Encoding.UTF8.GetBytes(actionMappingsString);
            this.blobClientMock.Setup(x => x.GetBlobData(It.IsNotNull<string>())).ReturnsAsync(actionMappingBlobData);
            newActionMapping.ActionId = "actionYYYid";
            actionMappings.Add(newActionMapping);
            actionMappingsString = JsonConvert.SerializeObject(actionMappings);
            this.blobClientMock.Setup(
                x => x.UploadFromByteArrayAsync(
                        It.IsNotNull<string>(),
                        It.IsNotNull<byte[]>(), 
                        It.IsNotNull<int>(),
                        It.IsNotNull<int>(),
                        It.IsNotNull<AccessCondition>(),
                        It.IsAny<BlobRequestOptions>(),
                        It.IsAny<OperationContext>()))
                .Callback<string, byte[], int, int, AccessCondition, BlobRequestOptions, OperationContext>(
                    (a, b, c, d, e, f, g) => saveBuf = Encoding.UTF8.GetString(b))
                .Returns(Task.FromResult(true));
            await this.actionMappingRepository.SaveMappingAsync(newActionMapping);
            Assert.NotNull(saveBuf);
            Assert.Equal(actionMappingsString, saveBuf);
        }
    }
}
