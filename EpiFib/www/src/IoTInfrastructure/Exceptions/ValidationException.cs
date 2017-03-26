// <copyright file="ValidationException.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace IoTInfrastructure.Exceptions
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class ValidationException : DeviceAdministrationExceptionBase
    {
        public ValidationException(string deviceId) : base(deviceId)
        {
            this.Errors = new List<string>();
        }

        public ValidationException(string deviceId, Exception innerException) : base(deviceId, innerException)
        {
            this.Errors = new List<string>();
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        protected ValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            this.Errors = (IList<string>)info.GetValue("Errors", typeof(IList<string>));
        }

        public IList<string> Errors { get; set; }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            info.AddValue("Errors", this.Errors, typeof(IList<string>));
            base.GetObjectData(info, context);
        }
    }
}
