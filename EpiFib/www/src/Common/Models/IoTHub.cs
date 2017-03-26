// <copyright file="IoTHub.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace Common.Models
{
    using System;

    public class IoTHub
    {
        public string MessageId { get; set; }

        public string CorrelationId { get; set; }

        public string ConnectionDeviceId { get; set; }

        public string ConnectionDeviceGenerationId { get; set; }

        public DateTime EnqueuedTime { get; set; }

        public string StreamId { get; set; }

        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }
}
