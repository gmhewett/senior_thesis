// <copyright file="IoTHubRepositoryTests.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace UnitTests.Infrastructure
{
    using System;
    using System.Threading.Tasks;
    using global::Common.Models;
    using global::IoTInfrastructure.Repository;
    using Microsoft.Azure.Devices;
    using Moq;
    using Ploeh.AutoFixture;
    using Xunit;

    public class IoTHubRepositoryTests
    {
        private readonly Mock<IIoTHubDeviceManager> deviceManagerMock;
        private readonly Fixture fixture;
        private readonly IIoTHubRepository iotHubRepository;

        public IoTHubRepositoryTests()
        {
            this.deviceManagerMock = new Mock<IIoTHubDeviceManager>();
            this.deviceManagerMock.Setup(dm => dm.AddDeviceAsync(It.IsAny<Device>())).ReturnsAsync(new Device());
            this.deviceManagerMock.Setup(dm => dm.RemoveDeviceAsync(It.IsAny<string>())).Returns(Task.FromResult(true));
            this.iotHubRepository = new IoTHubRepository(this.deviceManagerMock.Object);
            this.fixture = new Fixture();
        }

        [Fact]
        public async void AddDeviceAsync()
        {
            var device = this.fixture.Create<DeviceModel>();
            var keys = new SecurityKeys("fbsIV6w7gfVUyoRIQFSVgw ==", "1fLjiNCMZF37LmHnjZDyVQ ==");
            var sameDevice = await this.iotHubRepository.AddDeviceAsync(device, keys);
            Assert.Equal(sameDevice, device);
        }

        [Fact]
        public async void TryAddDeviceAsync()
        {
            var device = new Device("deviceId")
            {
                Authentication = null,
                Status = new DeviceStatus()
            };

            var result = await this.iotHubRepository.TryAddDeviceAsync(device);
            Assert.True(result);

            result = await this.iotHubRepository.TryAddDeviceAsync(null);
            Assert.False(result);
        }

        [Fact]
        public async void GetIotHubDeviceAsync()
        {
            var deviceId = this.fixture.Create<string>();
            this.deviceManagerMock.Setup(dm => dm.GetDeviceAsync(deviceId))
                .ReturnsAsync(new Device(deviceId));
            var d = await this.iotHubRepository.GetIotHubDeviceAsync(deviceId);
            Assert.Equal(deviceId, d.Id);
        }

        [Fact]
        public async void RemoveDeviceAsync()
        {
            var deviceId = this.fixture.Create<string>();
            await this.iotHubRepository.RemoveDeviceAsync(deviceId);
            this.deviceManagerMock.Verify(mock => mock.RemoveDeviceAsync(deviceId), Times.Once());
        }

        [Fact]
        public async void TryRemoveDeviceAsync()
        {
            var deviceId = this.fixture.Create<string>();
            var result = await this.iotHubRepository.TryRemoveDeviceAsync(deviceId);
            Assert.True(result);

            this.deviceManagerMock.Setup(dm => dm.RemoveDeviceAsync(It.IsAny<string>())).Throws(new Exception());
            result = await this.iotHubRepository.TryRemoveDeviceAsync(null);
            Assert.False(result);
        }

        [Fact]
        public async void UpdateDeviceEnabledStatusAsync()
        {
            var deviceId = this.fixture.Create<string>();
            var device = new Device(deviceId) { Status = DeviceStatus.Enabled };

            this.deviceManagerMock.Setup(dm => dm.GetDeviceAsync(deviceId))
                .ReturnsAsync(device);

            this.deviceManagerMock.Setup(dm => dm.UpdateDeviceAsync(It.IsAny<Device>()))
                .ReturnsAsync(device);

            var sameDevice = await this.iotHubRepository.UpdateDeviceEnabledStatusAsync(deviceId, false);
            Assert.Equal(sameDevice.Status, DeviceStatus.Disabled);

            sameDevice = await this.iotHubRepository.UpdateDeviceEnabledStatusAsync(deviceId, true);
            Assert.Equal(sameDevice.Status, DeviceStatus.Enabled);
        }

        [Fact]
        public async void SendCommand()
        {
            var commandHistory = this.fixture.Create<CommandHistory>();
            var deviceId = this.fixture.Create<string>();
            this.deviceManagerMock.Setup(dm => dm.SendAsync(deviceId, It.IsAny<Message>())).Returns(Task.FromResult(true));
            this.deviceManagerMock.Setup(dm => dm.CloseAsyncDevice()).Returns(Task.FromResult(true));

            await this.iotHubRepository.SendCommand(deviceId, commandHistory);
            this.deviceManagerMock.Verify(mock => mock.SendAsync(deviceId, It.IsAny<Message>()), Times.Once());
            this.deviceManagerMock.Verify(mock => mock.CloseAsyncDevice(), Times.Once());
        }

        [Fact]
        public async void GetDeviceKeysAsync()
        {
            var deviceId = this.fixture.Create<string>();
            var device = new Device(deviceId);
            var auth = new AuthenticationMechanism
            {
                SymmetricKey = new SymmetricKey
                {
                    PrimaryKey = "1fLjiNCMZF37LmHnjZDyVQ ==",
                    SecondaryKey = "fbsIV6w7gfVUyoRIQFSVgw =="
                }
            };
            device.Authentication = auth;
            string id = deviceId;
            this.deviceManagerMock.Setup(dm => dm.GetDeviceAsync(id))
                .ReturnsAsync(device);

            var keys = await this.iotHubRepository.GetDeviceKeysAsync(deviceId);
            Assert.NotNull(keys);
            deviceId = this.fixture.Create<string>();
            this.deviceManagerMock.Setup(dm => dm.GetDeviceAsync(deviceId))
                .ReturnsAsync(null as Device);

            keys = await this.iotHubRepository.GetDeviceKeysAsync(deviceId);
            Assert.Null(keys);
        }
    }
}
