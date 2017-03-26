// <copyright file="EpiFibTelemetry.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace Simulator.WebJob.SimulatorCore.Telemetry
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Models.SampleDataGenerator;
    using Simulator.WebJob.SimulatorCore.Logging;

    public class EpiFibTelemetry : IEpiFibTelemetry
    {
        private const int ReportFrequencyInSeconds = 60;
        private const int PeakFrequencyInSeconds = 120;

        private readonly ILogger logger;
        private readonly string deviceId;
        private readonly SampleDataGenerator temperatureGenerator;
        private readonly SampleDataGenerator humidityGenerator;
        private readonly SampleDataGenerator externalTemperatureGenerator;

        public EpiFibTelemetry(ILogger logger, string deviceId)
        {
            this.logger = logger;
            this.deviceId = deviceId;

            this.ActivateExternalTemperature = false;
            this.TelemetryActive = true;

            int peakFrequencyInTicks = Convert.ToInt32(Math.Ceiling((double)PeakFrequencyInSeconds / ReportFrequencyInSeconds));

            this.temperatureGenerator = new SampleDataGenerator(33, 36, 42, peakFrequencyInTicks);
            this.humidityGenerator = new SampleDataGenerator(20, 50);
            this.externalTemperatureGenerator = new SampleDataGenerator(-20, 120);
        }

        public bool ActivateExternalTemperature { get; set; }

        public bool TelemetryActive { get; set; }

        public async Task SendEventsAsync(CancellationToken token, Func<object, Task> sendMessageAsync)
        {
            var monitorData = new EpiFibTelemetryData();
            while (!token.IsCancellationRequested)
            {
                if (this.TelemetryActive)
                {
                    monitorData.DeviceId = this.deviceId;
                    monitorData.Temperature = this.temperatureGenerator.GetNextValue();
                    monitorData.Humidity = this.humidityGenerator.GetNextValue();
                    string messageBody = "Temperature: " + Math.Round(monitorData.Temperature, 2)
                                         + " Humidity: " + Math.Round(monitorData.Humidity, 2);

                    if (this.ActivateExternalTemperature)
                    {
                        monitorData.ExternalTemperature = this.externalTemperatureGenerator.GetNextValue();
                        messageBody += " External Temperature: " + Math.Round((double)monitorData.ExternalTemperature, 2);
                    }
                    else
                    {
                        monitorData.ExternalTemperature = null;
                    }

                    this.logger.LogInfo("Sending " + messageBody + " for Device: " + this.deviceId);

                    await sendMessageAsync(monitorData);
                }

                await Task.Delay(TimeSpan.FromSeconds(ReportFrequencyInSeconds), token);
            }
        }

        public void ChangeSetPointTemperature(double newSetPointTemperature)
        {
            this.temperatureGenerator.ShiftSubsequentData(newSetPointTemperature);
        }
    }
}
