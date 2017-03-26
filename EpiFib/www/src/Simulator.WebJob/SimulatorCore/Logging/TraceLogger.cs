// <copyright file="TraceLogger.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace Simulator.WebJob.SimulatorCore.Logging
{
    using System.Diagnostics;

    public class TraceLogger : ILogger
    {
        public void LogInfo(string message)
        {
            Trace.TraceInformation(message);
        }

        public void LogInfo(string format, params object[] args)
        {
            Trace.TraceInformation(format, args);
        }

        public void LogWarning(string message)
        {
            Trace.TraceWarning(message);
        }

        public void LogWarning(string format, params object[] args)
        {
            Trace.TraceWarning(format, args);
        }

        public void LogError(string message)
        {
            Trace.TraceError(message);
        }

        public void LogError(string format, params object[] args)
        {
            Trace.TraceError(format, args);
        }
    }
}
