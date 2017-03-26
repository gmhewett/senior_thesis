// <copyright file="IRandomGenerator.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace Common.Models.SampleDataGenerator
{
    public interface IRandomGenerator
    {
        double GetRandomDouble();
    }
}
