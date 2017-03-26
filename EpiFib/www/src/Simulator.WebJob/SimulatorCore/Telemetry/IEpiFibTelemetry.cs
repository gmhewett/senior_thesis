// <copyright file="IEpiFibTelemetry.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>
namespace Simulator.WebJob.SimulatorCore.Telemetry
{
    public interface IEpiFibTelemetry : ITelemetry
    {
        bool ActivateExternalTemperature { get; set; }

        bool TelemetryActive { get; set; }

        void ChangeSetPointTemperature(double newSetPointTemperature);
    }
}
