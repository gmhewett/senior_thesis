// <copyright file="IMessageFeedbackProcessor.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace EventProcessor.WebJob.Processors
{
    public interface IMessageFeedbackProcessor
    {
        void Start();

        void Stop();
    }
}
