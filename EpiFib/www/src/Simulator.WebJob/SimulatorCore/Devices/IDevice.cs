// <copyright file="IDevice.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace Simulator.WebJob.SimulatorCore.Devices
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Models;
    using Simulator.WebJob.SimulatorCore.Telemetry;

    public interface IDevice
    {
        string DeviceID { get; set; }

        string HostName { get; set; }

        string PrimaryAuthKey { get; set; }

        DeviceProperties DeviceProperties { get; set; }

        List<Command> Commands { get; set; }

        List<ITelemetry> TelemetryEvents { get; }

        bool RepeatEventListForever { get; set; }

        void Init(InitialDeviceConfig config);

        Task SendDeviceInfo();

        DeviceModel GetDeviceInfo();

        Task StartAsync(CancellationToken token);
    }
}
