// <copyright file="DeviceServiceTests.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace UnitTests.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using global::Common.Configuration;
    using global::Common.Exceptions;
    using global::Common.Models;
    using global::Common.Repository;
    using global::IoTInfrastructure.Exceptions;
    using global::IoTInfrastructure.Models;
    using global::IoTInfrastructure.Repository;
    using global::IoTInfrastructure.Services;
    using Microsoft.Azure.Devices;
    using Moq;
    using Newtonsoft.Json;
    using Ploeh.AutoFixture;
    using Xunit;

    public class DeviceServiceTests
    {
        private readonly Mock<IConfigurationProvider> configProviderMock;
        private readonly IDeviceService deviceService;
        private readonly Mock<IDeviceRegistryCrudRepository> deviceRegistryCrudRepositoryMock;
        private readonly Mock<IDeviceRegistryListRepository> deviceRegistryListRepositoryMock;
        private readonly Mock<IDeviceRulesService> deviceRulesLogicMock;
        private readonly Mock<IIoTHubRepository> ioTHubRepositoryMock;
        private readonly Mock<ISecurityKeyGenerator> securityKeyGeneratorMock;
        private readonly Mock<IVirtualDeviceStorage> virtualDeviceStorageMock;
        private readonly Fixture fixture;

        public DeviceServiceTests()
        {
            this.ioTHubRepositoryMock = new Mock<IIoTHubRepository>();
            this.deviceRegistryCrudRepositoryMock = new Mock<IDeviceRegistryCrudRepository>();
            this.deviceRegistryListRepositoryMock = new Mock<IDeviceRegistryListRepository>();
            this.virtualDeviceStorageMock = new Mock<IVirtualDeviceStorage>();
            this.configProviderMock = new Mock<IConfigurationProvider>();
            this.securityKeyGeneratorMock = new Mock<ISecurityKeyGenerator>();
            this.deviceRulesLogicMock = new Mock<IDeviceRulesService>();
            this.deviceService = new DeviceService(
                this.configProviderMock.Object, 
                this.ioTHubRepositoryMock.Object,
                this.deviceRegistryCrudRepositoryMock.Object,
                this.deviceRegistryListRepositoryMock.Object,
                this.virtualDeviceStorageMock.Object,
                this.securityKeyGeneratorMock.Object,
                this.deviceRulesLogicMock.Object);
            this.fixture = new Fixture();
        }

        [Fact]
        public async Task GetDevicesTest()
        {
            var query = this.fixture.Create<DeviceListQuery>();
            var result = this.fixture.Create<DeviceListQueryResult>();
            this.deviceRegistryListRepositoryMock.SetupSequence(x => x.GetDeviceList(It.IsAny<DeviceListQuery>()))
                .ReturnsAsync(result)
                .ReturnsAsync(new DeviceListQueryResult());

            var res = await this.deviceService.GetDevices(query);
            Assert.NotNull(res);
            Assert.NotNull(res.Results);
            Assert.NotEqual(0, res.TotalDeviceCount);
            Assert.NotEqual(0, res.TotalFilteredCount);

            res = await this.deviceService.GetDevices(null);
            Assert.NotNull(res);
            Assert.Null(res.Results);
            Assert.Equal(0, res.TotalDeviceCount);
            Assert.Equal(0, res.TotalFilteredCount);
        }

        [Fact]
        public async Task GetDeviceAsyncTest()
        {
            var device = this.fixture.Create<DeviceModel>();
            this.deviceRegistryCrudRepositoryMock.Setup(x => x.GetDeviceAsync(device.DeviceProperties.DeviceID))
                .ReturnsAsync(device);

            var retDevice = await this.deviceService.GetDeviceAsync(device.DeviceProperties.DeviceID);
            Assert.Equal(device, retDevice);

            retDevice = await this.deviceService.GetDeviceAsync("DeviceNotExist");
            Assert.Null(retDevice);

            retDevice = await this.deviceService.GetDeviceAsync(null);
            Assert.Null(retDevice);
        }

        [Fact]
        public async void AddDeviceAsyncTest()
        {
            var d1 = this.fixture.Create<DeviceModel>();
            this.ioTHubRepositoryMock.Setup(x => x.AddDeviceAsync(It.IsAny<DeviceModel>(), It.IsAny<SecurityKeys>()))
                .ReturnsAsync(d1);

            // Add device without DeviceProperties
            d1.DeviceProperties = null;
            await Assert.ThrowsAsync<ValidationException>(async () => await this.deviceService.AddDeviceAsync(d1));

            // Add device with Null or empty DeviceId
            d1.DeviceProperties = this.fixture.Create<DeviceProperties>();
            d1.DeviceProperties.DeviceID = null;
            await Assert.ThrowsAsync<ValidationException>(async () => await this.deviceService.AddDeviceAsync(d1));
            d1.DeviceProperties.DeviceID = string.Empty;
            await Assert.ThrowsAsync<ValidationException>(async () => await this.deviceService.AddDeviceAsync(d1));

            // Add existing device
            var d2 = this.fixture.Create<DeviceModel>();
            this.deviceRegistryCrudRepositoryMock.Setup(x => x.GetDeviceAsync(d2.DeviceProperties.DeviceID))
                .ReturnsAsync(d2);
            await Assert.ThrowsAsync<ValidationException>(async () => await this.deviceService.AddDeviceAsync(d2));

            d1.DeviceProperties.DeviceID = this.fixture.Create<string>();
            var keys = new SecurityKeys("fbsIV6w7gfVUyoRIQFSVgw ==", "1fLjiNCMZF37LmHnjZDyVQ ==");
            this.securityKeyGeneratorMock.Setup(x => x.CreateRandomKeys()).Returns(keys);
            var hostname = this.fixture.Create<string>();
            this.configProviderMock.Setup(x => x.GetConfigurationSettingValue(It.IsAny<string>())).Returns(hostname);

            // Device registry throws exception
            this.deviceRegistryCrudRepositoryMock.Setup(x => x.AddDeviceAsync(It.IsAny<DeviceModel>()))
                .ThrowsAsync(new Exception());
            this.ioTHubRepositoryMock.Setup(x => x.TryRemoveDeviceAsync(It.IsAny<string>())).ReturnsAsync(true).Verifiable();
            await Assert.ThrowsAsync<Exception>(async () => await this.deviceService.AddDeviceAsync(d1));
            this.virtualDeviceStorageMock.Verify(x => x.AddOrUpdateDeviceAsync(It.IsAny<InitialDeviceConfig>()), Times.Never());
            this.ioTHubRepositoryMock.Verify(x => x.TryRemoveDeviceAsync(d1.DeviceProperties.DeviceID), Times.Once());

            // Custom device
            d1.IsSimulatedDevice = false;
            this.deviceRegistryCrudRepositoryMock.Setup(x => x.AddDeviceAsync(It.IsAny<DeviceModel>())).ReturnsAsync(d1);
            var ret = await this.deviceService.AddDeviceAsync(d1);
            this.virtualDeviceStorageMock.Verify(x => x.AddOrUpdateDeviceAsync(It.IsAny<InitialDeviceConfig>()), Times.Never());
            Assert.NotNull(ret);
            Assert.Equal(d1, ret.Device);
            Assert.Equal(keys, ret.SecurityKeys);

            // Simulated device
            this.deviceRegistryCrudRepositoryMock.Setup(x => x.AddDeviceAsync(It.IsAny<DeviceModel>())).ReturnsAsync(d1);
            this.virtualDeviceStorageMock.Setup(x => x.AddOrUpdateDeviceAsync(It.IsAny<InitialDeviceConfig>())).Verifiable();
            d1.IsSimulatedDevice = true;
            ret = await this.deviceService.AddDeviceAsync(d1);
            this.virtualDeviceStorageMock.Verify(x => x.AddOrUpdateDeviceAsync(It.IsAny<InitialDeviceConfig>()), Times.Once());
            Assert.NotNull(ret);
            Assert.Equal(d1, ret.Device);
            Assert.Equal(keys, ret.SecurityKeys);
        }

        [Fact]
        public async void RemoveDeviceAsyncTest()
        {
            var deviceId = this.fixture.Create<string>();
            var device = new Device(deviceId);
            this.ioTHubRepositoryMock.Setup(x => x.GetIotHubDeviceAsync(It.IsNotNull<string>())).ReturnsAsync(device);
            this.ioTHubRepositoryMock.Setup(x => x.RemoveDeviceAsync(It.IsNotNull<string>())).Returns(Task.FromResult(true));

            // Device not registered with IoTHub
            await Assert.ThrowsAsync<DeviceNotRegisteredException>(async () => await this.deviceService.RemoveDeviceAsync(null));

            // Should pass without any exceptions
            this.virtualDeviceStorageMock.Setup(x => x.RemoveDeviceAsync(It.IsNotNull<string>())).ReturnsAsync(true);
            this.deviceRulesLogicMock.Setup(x => x.RemoveAllRulesForDeviceAsync(It.IsNotNull<string>())).ReturnsAsync(true);
            await this.deviceService.RemoveDeviceAsync(deviceId);
            this.virtualDeviceStorageMock.Verify(x => x.RemoveDeviceAsync(deviceId), Times.Once());
            this.deviceRulesLogicMock.Verify(x => x.RemoveAllRulesForDeviceAsync(deviceId), Times.Once());

            this.deviceRegistryCrudRepositoryMock.Setup(x => x.RemoveDeviceAsync(It.IsAny<string>()))
                .Throws(new Exception());
            this.ioTHubRepositoryMock.Setup(x => x.TryAddDeviceAsync(It.IsAny<Device>())).ReturnsAsync(true).Verifiable();
            await Assert.ThrowsAsync<Exception>(async () => await this.deviceService.RemoveDeviceAsync(deviceId));
            this.ioTHubRepositoryMock.Verify(x => x.TryAddDeviceAsync(device), Times.Once());
        }

        [Fact]
        public async void UpdateDeviceAsyncTest()
        {
            var d = this.fixture.Create<DeviceModel>();
            this.deviceRegistryCrudRepositoryMock.Setup(x => x.UpdateDeviceAsync(It.IsNotNull<DeviceModel>()))
                .ReturnsAsync(d);

            var r = await this.deviceService.UpdateDeviceAsync(d);
            Assert.Equal(d, r);
        }

        [Fact]
        public async void UpdateDeviceFromDeviceInfoPacketAsyncTest()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await this.deviceService.UpdateDeviceFromDeviceInfoPacketAsync(null));

            var d = this.fixture.Create<DeviceModel>();
            this.deviceRegistryCrudRepositoryMock.Setup(x => x.GetDeviceAsync(d.IoTHub.ConnectionDeviceId))
                .ReturnsAsync(d);
            this.deviceRegistryCrudRepositoryMock.Setup(x => x.UpdateDeviceAsync(It.IsAny<DeviceModel>()))
                .ReturnsAsync(d);
            var r = await this.deviceService.UpdateDeviceFromDeviceInfoPacketAsync(d);
            Assert.Equal(d, r);

            d.SystemProperties = null;
            d.Telemetry = null;
            d.Commands = null;
            r = await this.deviceService.UpdateDeviceFromDeviceInfoPacketAsync(d);
            Assert.Equal(d, r);
        }

        [Fact]
        public async void SendCommandAsyncTest()
        {
            var d = this.fixture.Create<DeviceModel>();
            this.deviceRegistryCrudRepositoryMock.Setup(x => x.GetDeviceAsync(d.DeviceProperties.DeviceID))
                .ReturnsAsync(d);

            // Invalid device
            await Assert.ThrowsAsync<DeviceNotRegisteredException>(
                async () => await this.deviceService.SendCommandAsync(null, null, null));

            // Invalid command
            await Assert.ThrowsAsync<UnsupportedCommandException>(
                async () => await this.deviceService.SendCommandAsync(d.DeviceProperties.DeviceID, "Invalid command", null));

            // Valid command
            this.ioTHubRepositoryMock.Setup(x => x.SendCommand(It.IsNotNull<string>(), It.IsNotNull<CommandHistory>()))
                .Returns(Task.FromResult(true));
            this.deviceRegistryCrudRepositoryMock.Setup(x => x.UpdateDeviceAsync(It.IsNotNull<DeviceModel>()))
                .ReturnsAsync(new DeviceModel());
            await this.deviceService.SendCommandAsync(d.DeviceProperties.DeviceID, d.Commands[0].Name, null);
        }

        [Fact]
        public void ExtractLocationsDataTest()
        {
            var listOfDevices = this.fixture.Create<List<DeviceModel>>();
            var latitudes = new List<double>();
            var longitudes = new List<double>();
            var locations = new List<DeviceLocationModel>();
            foreach (var d in listOfDevices)
            {
                try
                {
                    latitudes.Add(d.DeviceProperties.Latitude.Value);
                    longitudes.Add(d.DeviceProperties.Longitude.Value);
                    locations.Add(new DeviceLocationModel
                    {
                        DeviceId = d.DeviceProperties.DeviceID,
                        Latitude = d.DeviceProperties.Latitude.Value,
                        Longitude = d.DeviceProperties.Longitude.Value
                    });
                }
                catch
                {
                    // ignored
                }
            }

            var offset = 0.05;
            var minLat = latitudes.Min() - offset;
            var maxLat = latitudes.Max() + offset;
            var minLong = longitudes.Min() - offset;
            var maxLong = longitudes.Max() + offset;

            var res = this.deviceService.ExtractLocationsData(listOfDevices);
            Assert.NotNull(res);
            Assert.Equal(JsonConvert.SerializeObject(locations), JsonConvert.SerializeObject(res.DeviceLocationList));
            Assert.Equal(minLat, res.MinimumLatitude);
            Assert.Equal(maxLat, res.MaximumLatitude);
            Assert.Equal(minLong, res.MinimumLongitude);
            Assert.Equal(maxLong, res.MaximumLongitude);

            res = this.deviceService.ExtractLocationsData(null);
            Assert.NotNull(res);
            Assert.Equal(
                JsonConvert.SerializeObject(new List<DeviceLocationModel>()),
                JsonConvert.SerializeObject(res.DeviceLocationList));
            Assert.Equal(47.6 - offset, res.MinimumLatitude);
            Assert.Equal(47.6 + offset, res.MaximumLatitude);
            Assert.Equal(-122.3 - offset, res.MinimumLongitude);
            Assert.Equal(-122.3 + offset, res.MaximumLongitude);
        }

        [Fact]
        public void ExtractTelemetryTest()
        {
            var d = this.fixture.Create<DeviceModel>();
            var exp = d.Telemetry.Select(t => new DeviceTelemetryFieldModel
            {
                DisplayName = t.DisplayName,
                Name = t.Name,
                Type = t.Type
            }).ToList();

            Assert.Null(this.deviceService.ExtractTelemetry(null));

            var res = this.deviceService.ExtractTelemetry(d);
            Assert.Equal(JsonConvert.SerializeObject(exp), JsonConvert.SerializeObject(res));
        }

        [Fact]
        public async void UpdateDeviceEnabledStatusAsyncTest_customDevice()
        {
            string deviceId = this.fixture.Create<string>();
            bool isEnabled = this.fixture.Create<bool>();
            var device = this.fixture.Create<DeviceModel>();
            device.IsSimulatedDevice = false;
            this.ioTHubRepositoryMock.Setup(mock => mock.UpdateDeviceEnabledStatusAsync(deviceId, isEnabled)).ReturnsAsync(new Device());
            this.deviceRegistryCrudRepositoryMock.SetupSequence(mock => mock.UpdateDeviceEnabledStatusAsync(deviceId, isEnabled))
                .ReturnsAsync(device)
                .Throws<Exception>();
            var res = await this.deviceService.UpdateDeviceEnabledStatusAsync(deviceId, isEnabled);
            Assert.Equal(res, device);

            this.ioTHubRepositoryMock.Setup(mock => mock.UpdateDeviceEnabledStatusAsync(deviceId, !isEnabled)).ReturnsAsync(new Device());
            await Assert.ThrowsAsync<Exception>(async () => await this.deviceService.UpdateDeviceEnabledStatusAsync(deviceId, isEnabled));
        }

        [Fact]
        public async void UpdateDeviceEnabledStatusAsyncTest_simulatedDevice()
        {
            var device = this.fixture.Create<DeviceModel>();
            var keys = this.fixture.Create<SecurityKeys>();
            var hostname = "hostname";
            device.IsSimulatedDevice = true;
            var deviceId = device.DeviceProperties.DeviceID;
            InitialDeviceConfig savedConfig = null;
            this.configProviderMock.Setup(x => x.GetConfigurationSettingValue(It.IsAny<string>())).Returns(hostname);
            this.ioTHubRepositoryMock.Setup(mock => mock.UpdateDeviceEnabledStatusAsync(deviceId, It.IsAny<bool>())).ReturnsAsync(new Device());
            this.ioTHubRepositoryMock.Setup(mock => mock.GetDeviceKeysAsync(deviceId)).ReturnsAsync(keys);
            this.deviceRegistryCrudRepositoryMock.Setup(
                mock => mock.UpdateDeviceEnabledStatusAsync(deviceId, It.IsAny<bool>()))
                .ReturnsAsync(device);
            this.virtualDeviceStorageMock.Setup(
                mock => mock.AddOrUpdateDeviceAsync(It.IsNotNull<InitialDeviceConfig>()))
                .Callback<InitialDeviceConfig>(conf => savedConfig = conf)
                .Returns(Task.FromResult(true))
                .Verifiable();

            // Enable simulated device
            var res = await this.deviceService.UpdateDeviceEnabledStatusAsync(deviceId, true);
            Assert.Equal(res, device);
            this.virtualDeviceStorageMock.Verify();
            Assert.Equal(deviceId, savedConfig.DeviceId);
            Assert.Equal(hostname, savedConfig.HostName);
            Assert.Equal(keys.PrimaryKey, savedConfig.Key);

            this.virtualDeviceStorageMock.Setup(mock => mock.RemoveDeviceAsync(deviceId))
                .Returns(Task.FromResult(true))
                .Verifiable();

            // Disable simulated device
            res = await this.deviceService.UpdateDeviceEnabledStatusAsync(deviceId, false);
            Assert.Equal(res, device);
            this.virtualDeviceStorageMock.Verify();
        }

        [Fact]
        public void ApplyDevicePropertyValueModelsTest()
        {
            var device = this.fixture.Create<DeviceModel>();
            var devicePropertyValueModels = this.fixture.Create<IEnumerable<DevicePropertyValueModel>>();
            this.deviceService.ApplyDevicePropertyValueModels(device, devicePropertyValueModels);

            Assert.Throws<ArgumentNullException>(() => this.deviceService.ApplyDevicePropertyValueModels(null, devicePropertyValueModels));
            Assert.Throws<ArgumentNullException>(() => this.deviceService.ApplyDevicePropertyValueModels(device, null));
            device.DeviceProperties = null;
            Assert.Throws<DeviceRequiredPropertyNotFoundException>(
                () => this.deviceService.ApplyDevicePropertyValueModels(device, devicePropertyValueModels));
        }

        [Fact]
        public void ExtractDevicePropertyValuesModelsTest()
        {
            var device = this.fixture.Create<DeviceModel>();
            this.configProviderMock.Setup(mock => mock.GetConfigurationSettingValue("iotHub.HostName")).Returns("hostName");
            var res = this.deviceService.ExtractDevicePropertyValuesModels(device);
            Assert.Equal(res.Count(), 19);
            Assert.Equal(res.Last().Name, "HostName");
            Assert.Equal(res.Last().Value, "hostName");

            Assert.Throws<ArgumentNullException>(() => this.deviceService.ExtractDevicePropertyValuesModels(null));
            device.DeviceProperties = null;
            Assert.Throws<DeviceRequiredPropertyNotFoundException>(
                () => this.deviceService.ExtractDevicePropertyValuesModels(device));
        }

        [Fact]
        public async void GenerateNDevicesTest()
        {
            var device = this.fixture.Create<DeviceModel>();
            this.ioTHubRepositoryMock.Setup(mock => mock.AddDeviceAsync(It.IsAny<DeviceModel>(), It.IsAny<SecurityKeys>()))
                .ReturnsAsync(new DeviceModel());

            this.deviceRegistryCrudRepositoryMock.Setup(mock => mock.AddDeviceAsync(It.IsAny<DeviceModel>())).ReturnsAsync(device);
            this.configProviderMock.Setup(mock => mock.GetConfigurationSettingValue("iotHub.HostName")).Returns("hostName");
            this.virtualDeviceStorageMock.Setup(mock => mock.AddOrUpdateDeviceAsync(It.IsAny<InitialDeviceConfig>())).Returns(Task.FromResult(true));

            await this.deviceService.GenerateNDevices(10);
            this.deviceRegistryCrudRepositoryMock.Verify(mock => mock.AddDeviceAsync(It.IsAny<DeviceModel>()), Times.Exactly(10));
        }
    }
}
