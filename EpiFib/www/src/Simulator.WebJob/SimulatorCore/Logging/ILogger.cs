// <copyright file="ILogger.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace Simulator.WebJob.SimulatorCore.Logging
{
    public interface ILogger
    {
        void LogInfo(string message);

        void LogInfo(string format, params object[] args);

        void LogWarning(string message);

        void LogWarning(string format, params object[] args);

        void LogError(string message);

        void LogError(string format, params object[] args);
    }
}
