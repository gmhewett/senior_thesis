// <copyright file="DeviceRegistryRepositoryTests.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace UnitTests.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using global::Common.Exceptions;
    using global::Common.Helpers;
    using global::Common.Models;
    using IoTInfrastructure.Exceptions;
    using IoTInfrastructure.Repository;
    using Moq;
    using Ploeh.AutoFixture;
    using Xunit;

    public class DeviceRegistryRepositoryTests
    {
        private readonly Fixture fixture;
        private readonly DeviceRegistryRepository deviceRegistryRepository;
        private readonly Mock<IDeviceDocumentDbClient<DeviceModel>> mockDocumentDbClient;

        public DeviceRegistryRepositoryTests()
        {
            this.fixture = new Fixture();
            this.mockDocumentDbClient = new Mock<IDeviceDocumentDbClient<DeviceModel>>();
            this.deviceRegistryRepository = new DeviceRegistryRepository(this.mockDocumentDbClient.Object);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task GetDeviceAsync_should_throw_argument_exception_when_device_id_is_invalid(string deviceId)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => this.deviceRegistryRepository.GetDeviceAsync(deviceId));
        }

        [Fact]
        public async Task GetDeviceAsync_should_get_device_from_client()
        {
            var devices = this.fixture.Create<List<DeviceModel>>();
            var deviceId = devices.First().DeviceProperties.DeviceID;

            this.mockDocumentDbClient.Setup(x => x.QueryAsync())
                .ReturnsAsync(devices.AsQueryable());

            var device = await this.deviceRegistryRepository.GetDeviceAsync(deviceId);

            Assert.NotNull(device);
            Assert.Same(devices[0], device);
        }

        [Fact]
        public async Task GetDeviceAsync_should_return_null_when_device_id_is_not_found()
        {
            var devices = this.fixture.Create<List<DeviceModel>>();

            this.mockDocumentDbClient
                .Setup(x => x.QueryAsync())
                .ReturnsAsync(devices.AsQueryable());

            var device = await this.deviceRegistryRepository.GetDeviceAsync("foobarbaz");

            Assert.Null(device);
        }

        [Fact]
        public async Task AddDeviceAsync_should_throw_argument_null_when_device_is_null()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => this.deviceRegistryRepository.AddDeviceAsync(null));
        }

        [Fact]
        public async Task AddDeviceAsync_should_throw_device_exists_exception_when_device_with_same_id_exists()
        {
            var devices = this.fixture.Create<List<DeviceModel>>();
            this.mockDocumentDbClient
                .Setup(x => x.QueryAsync())
                .ReturnsAsync(devices.AsQueryable());

            await Assert.ThrowsAsync<DeviceAlreadyRegisteredException>(() => this.deviceRegistryRepository.AddDeviceAsync(devices.First()));
        }

        [Fact]
        public async Task AddDeviceAsync_should_add_device_to_db()
        {
            var device = this.fixture.Create<DeviceModel>();
            await this.deviceRegistryRepository.AddDeviceAsync(device);
            this.mockDocumentDbClient.Verify(x => x.SaveAsync(device));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task RemoveDeviceAsync_should_throw_argument_null_when_device_id_is_not_valid(string deviceId)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => this.deviceRegistryRepository.RemoveDeviceAsync(deviceId));
        }

        [Fact]
        public async Task RemoveDeviceAsync_should_throw_device_not_found_when_device_does_not_exist()
        {
            this.mockDocumentDbClient
                .Setup(x => x.QueryAsync())
                .ReturnsAsync(Enumerable.Empty<DeviceModel>().AsQueryable());

            await Assert.ThrowsAsync<DeviceNotRegisteredException>(() => this.deviceRegistryRepository.RemoveDeviceAsync("foobar"));

            this.mockDocumentDbClient.VerifyAll();
        }

        [Fact]
        public async Task RemoveDeviceAsync_should_remove_device_from_store()
        {
            var device = this.fixture.Create<DeviceModel>();

            this.mockDocumentDbClient
                .Setup(x => x.QueryAsync())
                .ReturnsAsync(new[] { device }.AsQueryable());

            await this.deviceRegistryRepository.RemoveDeviceAsync(device.DeviceProperties.DeviceID);

            this.mockDocumentDbClient.Verify(x => x.DeleteAsync(device.id, device.DeviceProperties.DeviceID));
        }

        [Fact]
        public async Task UpdateDeviceAsync_should_throw_argument_null_when_device_is_null()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => this.deviceRegistryRepository.UpdateDeviceAsync(null));
        }

        [Fact]
        public async Task UpdateDeviceAsync_should_throw_device_property_not_found_when_properties_is_null()
        {
            var device = this.fixture.Create<DeviceModel>();
            device.DeviceProperties = null;
            await Assert.ThrowsAsync<DeviceRequiredPropertyNotFoundException>(() => this.deviceRegistryRepository.UpdateDeviceAsync(device));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task UpdateDeviceAsync_should_throw_when_device_id_is_not_valid(string deviceId)
        {
            var device = this.fixture.Create<DeviceModel>();
            device.DeviceProperties.DeviceID = deviceId;
            await Assert.ThrowsAsync<DeviceRequiredPropertyNotFoundException>(() => this.deviceRegistryRepository.UpdateDeviceAsync(device));
        }

        [Fact]
        public async Task UpdateDeviceAsync_should_throw_device_not_found_when_device_does_not_exist()
        {
            var device = this.fixture.Create<DeviceModel>();

            this.mockDocumentDbClient
               .Setup(x => x.QueryAsync())
               .ReturnsAsync(Enumerable.Empty<DeviceModel>().AsQueryable());

            await Assert.ThrowsAsync<DeviceNotRegisteredException>(() => this.deviceRegistryRepository.UpdateDeviceAsync(device));
        }

        [Fact]
        public async Task UpdateDeviceAsync_should_set_db_id_properties_before_updating_to_store()
        {
            var existingDevice = this.fixture.Create<DeviceModel>();
            var updatingDevice = this.fixture.Create<DeviceModel>();

            existingDevice.DeviceProperties.DeviceID = updatingDevice.DeviceProperties.DeviceID;
            updatingDevice.id = null;
            updatingDevice._rid = null;

            this.mockDocumentDbClient
                .Setup(x => x.QueryAsync())
                .ReturnsAsync(new[] { existingDevice }.AsQueryable());

            await this.deviceRegistryRepository.UpdateDeviceAsync(updatingDevice);

            Assert.Equal(existingDevice.id, updatingDevice.id);
            Assert.Equal(existingDevice._rid, updatingDevice._rid);
        }

        [Fact]
        public async Task UpdateDeviceAsync_should_throw_invalid_operation_when_id_cannot_be_set_from_existing()
        {
            var existingDevice = this.fixture.Create<DeviceModel>();
            var updatingDevice = this.fixture.Create<DeviceModel>();

            existingDevice.DeviceProperties.DeviceID = updatingDevice.DeviceProperties.DeviceID;
            existingDevice.id = null;
            updatingDevice.id = null;

            this.mockDocumentDbClient
                .Setup(x => x.QueryAsync())
                .ReturnsAsync(new[] { existingDevice }.AsQueryable());

            await Assert.ThrowsAsync<InvalidOperationException>(() => this.deviceRegistryRepository.UpdateDeviceAsync(updatingDevice));
        }

        [Fact]
        public async Task UpdateDeviceAsync_should_throw_invalid_operation_when_rid_cannot_be_set_from_existing()
        {
            var existingDevice = this.fixture.Create<DeviceModel>();
            var updatingDevice = this.fixture.Create<DeviceModel>();

            existingDevice.DeviceProperties.DeviceID = updatingDevice.DeviceProperties.DeviceID;
            existingDevice._rid = null;
            updatingDevice._rid = null;

            this.mockDocumentDbClient
                .Setup(x => x.QueryAsync())
                .ReturnsAsync(new[] { existingDevice }.AsQueryable());

            await Assert.ThrowsAsync<InvalidOperationException>(() => this.deviceRegistryRepository.UpdateDeviceAsync(updatingDevice));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task UpdateDeviceEnabledStateStatus_should_throw_argument_null_when_device_id_is_not_valid(string deviceId)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => this.deviceRegistryRepository.UpdateDeviceEnabledStatusAsync(deviceId, true));
        }

        [Fact]
        public async Task UpdateDeviceEnabledStateStatus_should_throw_when_device_is_not_found()
        {
            this.mockDocumentDbClient
                .Setup(x => x.QueryAsync())
                .ReturnsAsync(Enumerable.Empty<DeviceModel>().AsQueryable());

            await Assert.ThrowsAsync<DeviceNotRegisteredException>(() => this.deviceRegistryRepository.UpdateDeviceEnabledStatusAsync("foobar", true));
        }

        [Fact]
        public async Task UpdateDeviceEnabledStateStatus_should_set_hub_enabled_and_update_in_store()
        {
            var device = this.fixture.Create<DeviceModel>();
            device.DeviceProperties.HubEnabledState = false;

            this.mockDocumentDbClient
                .Setup(x => x.QueryAsync())
                .ReturnsAsync(new[] { device }.AsQueryable());

            await this.deviceRegistryRepository.UpdateDeviceEnabledStatusAsync(device.DeviceProperties.DeviceID, true);

            Assert.True(device.DeviceProperties.HubEnabledState);
            this.mockDocumentDbClient.Verify(x => x.SaveAsync(device));
        }
    }
}
