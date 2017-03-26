// <copyright file="SecurityKeyGeneratorTests.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace UnitTests.Infrastructure
{
    using global::Common.Models;
    using global::IoTInfrastructure.Services;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SecurityKeyGeneratorTests
    {
        [TestMethod]
        public void CreateRandomKeysTest()
        {
            ISecurityKeyGenerator securityKeyGenerator = new SecurityKeyGenerator();
            SecurityKeys keys1 = securityKeyGenerator.CreateRandomKeys();
            SecurityKeys keys2 = securityKeyGenerator.CreateRandomKeys();

            Assert.IsNotNull(keys1);
            Assert.AreNotEqual(keys1.PrimaryKey, keys1.SecondaryKey);
            Assert.AreNotEqual(keys1.PrimaryKey, keys2.PrimaryKey);
            Assert.AreNotEqual(keys1.SecondaryKey, keys2.SecondaryKey);
        }
    }
}
