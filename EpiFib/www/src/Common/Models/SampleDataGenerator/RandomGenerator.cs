// <copyright file="RandomGenerator.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace Common.Models.SampleDataGenerator
{
    using System;

    public class RandomGenerator : IRandomGenerator
    {
        private static readonly Random Random = BuildRandomSource();

        public double GetRandomDouble()
        {
            lock (Random)
            {
                return Random.NextDouble();
            }
        }

        private static Random BuildRandomSource()
        {
            return new Random(Guid.NewGuid().GetHashCode());
        }
    }
}
