// <copyright file="ContainerDevice.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace Simulator.WebJob.Container.Devices
{
    using System.Threading.Tasks;
    using Common.Configuration;
    using Simulator.WebJob.SimulatorCore.CommandProcessors;
    using Simulator.WebJob.SimulatorCore.Devices;
    using Simulator.WebJob.SimulatorCore.Logging;
    using Simulator.WebJob.SimulatorCore.Telemetry;
    using Simulator.WebJob.SimulatorCore.Transport;

    public class ContainerDevice : DeviceBase
    {
        public ContainerDevice(
            ILogger logger, 
            ITransportFactory transportFactory, 
            ITelemetryFactory telemetryFactory, 
            IConfigurationProvider configurationProvider) 
            : base(logger, transportFactory, telemetryFactory, configurationProvider)
        {
        }

        public void StartTelemetryData()
        {
            var remoteMonitorTelemetry = (IEpiFibTelemetry)TelemetryController;
            remoteMonitorTelemetry.TelemetryActive = true;
            Logger.LogInfo("Device {0}: Telemetry has started", this.DeviceID);
        }

        public void StopTelemetryData()
        {
            var remoteMonitorTelemetry = (IEpiFibTelemetry)TelemetryController;
            remoteMonitorTelemetry.TelemetryActive = false;
            Logger.LogInfo("Device {0}: Telemetry has stopped", this.DeviceID);
        }

        public void ChangeSetPointTemp(double setPointTemp)
        {
            var remoteMonitorTelemetry = (IEpiFibTelemetry)TelemetryController;
            remoteMonitorTelemetry.ChangeSetPointTemperature(setPointTemp);
            Logger.LogInfo("Device {0} temperature changed to {1}", this.DeviceID, setPointTemp);
        }

        public async Task ChangeDeviceState(string deviceState)
        {
            // simply update the DeviceState property and send updated device info packet
            DeviceProperties.DeviceState = deviceState;
            await this.SendDeviceInfo();
            Logger.LogInfo("Device {0} in {1} state", this.DeviceID, deviceState);
        }

        public void DiagnosticTelemetry(bool active)
        {
            var remoteMonitorTelemetry = (IEpiFibTelemetry)TelemetryController;
            remoteMonitorTelemetry.ActivateExternalTemperature = active;
            string externalTempActive = active ? "on" : "off";
            Logger.LogInfo("Device {0}: External Temperature: {1}", this.DeviceID, externalTempActive);
        }

        protected override void InitCommandProcessors()
        {
            var pingDeviceProcessor = new PingDeviceProcessor(this);
            var startCommandProcessor = new StartCommandProcessor(this);
            var stopCommandProcessor = new StopCommandProcessor(this);
            var diagnosticTelemetryCommandProcessor = new DiagnosticTelemetryCommandProcessor(this);
            var changeSetPointTempCommandProcessor = new ChangeSetPointTempCommandProcessor(this);
            var changeDeviceStateCommmandProcessor = new ChangeDeviceStateCommandProcessor(this);

            pingDeviceProcessor.NextCommandProcessor = startCommandProcessor;
            startCommandProcessor.NextCommandProcessor = stopCommandProcessor;
            stopCommandProcessor.NextCommandProcessor = diagnosticTelemetryCommandProcessor;
            diagnosticTelemetryCommandProcessor.NextCommandProcessor = changeSetPointTempCommandProcessor;
            changeSetPointTempCommandProcessor.NextCommandProcessor = changeDeviceStateCommmandProcessor;

            this.RootCommandProcessor = pingDeviceProcessor;
        }
    }
}
