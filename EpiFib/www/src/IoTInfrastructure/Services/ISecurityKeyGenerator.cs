// <copyright file="ISecurityKeyGenerator.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace IoTInfrastructure.Services
{
    using Common.Models;

    public interface ISecurityKeyGenerator
    {
        SecurityKeys CreateRandomKeys();
    }
}
