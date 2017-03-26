// <copyright file="SampleDataGenerator.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace Common.Models.SampleDataGenerator
{
    using System;
    using Common.Helpers;

    public class SampleDataGenerator
    {
        private const double MaximumFractionToChangePerTick = 0.10;

        private readonly bool generatePeaks;
        private readonly int peakInterval;
        private readonly double deltaValue;
        private readonly double thresholdWidth;
        private readonly IRandomGenerator randomGenerator;
        private readonly object sync = new object();

        private double minValueToGenerate;
        private double maxNonPeakValueToGenerate;
        private double startValue;
        private double nextValue;
        private long tickCounter;

        public SampleDataGenerator(
            double minValueToGenerate,
            double maxNonPeakValueToGenerate, 
            double minPeakValueToGenerate,
            int peakInterval, 
            IRandomGenerator randomGenerator)
        {
            if (minValueToGenerate >= maxNonPeakValueToGenerate)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxNonPeakValueToGenerate),
                    maxNonPeakValueToGenerate,
                    "maxNonPeakValueToGenerate must be greater than minValueToGenerate.");
            }

            if ((Math.Abs(minPeakValueToGenerate) > 0) &&
                (maxNonPeakValueToGenerate >= minPeakValueToGenerate))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(minPeakValueToGenerate),
                    minPeakValueToGenerate,
                    "If not 0, minPeakValueToGenerate must be greater than maxNonPeakValueToGenerate.");
            }

            // minPeakValueToGenerate is zero when peaks are not generated
            this.generatePeaks = Math.Abs(minPeakValueToGenerate) > 0;
            if (this.generatePeaks && peakInterval == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(this.peakInterval), "peakInterval cannot be 0.");
            }

            EFGuard.NotNull(randomGenerator, nameof(randomGenerator));

            this.minValueToGenerate = minValueToGenerate;
            this.maxNonPeakValueToGenerate = maxNonPeakValueToGenerate;

            // Scale up to ensure we exceed rather than equal threshold in all cases
            minPeakValueToGenerate = minPeakValueToGenerate * 1.01;

            this.peakInterval = peakInterval;

            // Start in the middle of the range
            this.startValue = (maxNonPeakValueToGenerate - minValueToGenerate) / 2.0 + minValueToGenerate;
            this.nextValue = this.startValue;

            this.deltaValue = (maxNonPeakValueToGenerate - minValueToGenerate) * MaximumFractionToChangePerTick;
            this.thresholdWidth = minPeakValueToGenerate - minValueToGenerate;

            this.tickCounter = 1;
            this.randomGenerator = randomGenerator;
        }

        public SampleDataGenerator(
            double minValueToGenerate,
            double maxNonPeakValueToGenerate,
            double minPeakValueToGenerate, 
            int peakInterval)
            : this(
                  minValueToGenerate, 
                  maxNonPeakValueToGenerate, 
                  minPeakValueToGenerate,
                  peakInterval, 
                  new RandomGenerator())
        {
        }

        public SampleDataGenerator(
            double minValueToGenerate, 
            double maxNonPeakValueToGenerate,
            IRandomGenerator randomGenerator)
            : this(minValueToGenerate, maxNonPeakValueToGenerate, 0, 0, randomGenerator)
        {
        }

        public SampleDataGenerator(
            double minValueToGenerate, 
            double maxNonPeakValueToGenerate)
            : this(minValueToGenerate, maxNonPeakValueToGenerate, 0, 0, new RandomGenerator())
        {
        }

        public double GetNextValue()
        {
            this.GetNextRawValue();
            bool determinePeak = this.generatePeaks && this.tickCounter % this.peakInterval == 0;
            this.tickCounter++;
            if (determinePeak)
            {
                return this.nextValue + this.thresholdWidth;
            }

            return this.nextValue;
        }

        /// <summary>
        /// Shift all subsequent data to a new mid-point 
        /// (shifts existing ranges and peaks to the new value)
        /// </summary>
        /// <param name="newMidPointOfRange">New mid-point in the expected data range</param>
        public void ShiftSubsequentData(double newMidPointOfRange)
        {
            lock (this.sync)
            {
                this.nextValue = newMidPointOfRange;

                double shift = this.startValue - this.minValueToGenerate;

                this.startValue = newMidPointOfRange;
                this.minValueToGenerate = newMidPointOfRange - shift;
                this.maxNonPeakValueToGenerate = newMidPointOfRange + shift;
            }
        }

        private void GetNextRawValue()
        {
            double adjustment = 2.0 * this.deltaValue * this.randomGenerator.GetRandomDouble() - this.deltaValue;
            this.nextValue += adjustment;
            if (this.nextValue < this.minValueToGenerate || this.nextValue > this.maxNonPeakValueToGenerate)
            {
                this.nextValue -= adjustment;

                // in case of cosmic ray or memory issues (or bugs), check and make sure we're back in the correct range, and fix if otherwise
                if (this.nextValue < this.minValueToGenerate || this.nextValue > this.maxNonPeakValueToGenerate)
                {
                    this.nextValue = this.startValue;
                }
            }
        }
    }
}
