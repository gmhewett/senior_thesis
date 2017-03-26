// <copyright file="ServiceResponse.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace IoTInfrastructure.Models
{
    using System.Collections.Generic;
    using Microsoft.Azure.Amqp.Framing;

    public class ServiceResponse<T>
    {
        public ServiceResponse()
        {
            this.Error = new List<Error>();
        }

        public List<Error> Error { get; set; }

        public T Data { get; set; }

        public bool ShouldSerializeError()
        {
            return this.Error.Count > 0;
        }
    }
}
