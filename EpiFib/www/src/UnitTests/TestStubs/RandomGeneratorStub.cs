// <copyright file="RandomGeneratorStub.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace UnitTests.TestStubs
{
    using global::Common.Models.SampleDataGenerator;

    public class RandomGeneratorStub : IRandomGenerator
    {
        private readonly double valueReturned;

        public RandomGeneratorStub(double valueReturned)
        {
            this.valueReturned = valueReturned;
        }

        public double GetRandomDouble()
        {
            return this.valueReturned;
        }
    }
}
