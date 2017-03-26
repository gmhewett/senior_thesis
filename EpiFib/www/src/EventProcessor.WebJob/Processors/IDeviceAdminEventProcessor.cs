// <copyright file="IDeviceAdminEventProcessor.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace EventProcessor.WebJob.Processors
{
    using System.Threading;

    public interface IDeviceAdminEventProcessor
    {
        void Start();

        void Stop();
    }
}
