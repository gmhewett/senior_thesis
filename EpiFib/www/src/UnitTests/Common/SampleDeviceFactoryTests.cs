// <copyright file="SampleDeviceFactoryTests.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace UnitTests.Common
{
    using System;
    using global::Common.Factory;
    using global::Common.Helpers;
    using global::IoTInfrastructure.Services;
    using Xunit;

    public class SampleDeviceFactoryTests
    {
        [Fact]
        public void TestGetDefaultDeviceNames()
        {
            var s = SampleDeviceFactory.GetDefaultDeviceNames();
            Assert.NotEmpty(s);
        }

        [Fact]
        public void TestGetSampleDevice()
        {
            var randomnumber = new Random();
            ISecurityKeyGenerator securityKeyGenerator = new SecurityKeyGenerator();
            var keys = securityKeyGenerator.CreateRandomKeys();
            var d = SampleDeviceFactory.GetSampleDevice(randomnumber, keys);
            Assert.NotNull(d);
            Assert.NotNull(d.DeviceProperties);
            Assert.NotNull(d.DeviceProperties.DeviceID);
        }

        [Fact]
        public void TestGetSampleSimulatedDevice()
        {
            var d = DeviceCreatorHelper.BuildDeviceStructure("test", true, null);
            Assert.NotNull(d);
            Assert.Equal("test", d.DeviceProperties.DeviceID);
            Assert.Equal("normal", d.DeviceProperties.DeviceState);
            Assert.Equal(null, d.DeviceProperties.HubEnabledState);
        }
    }
}
