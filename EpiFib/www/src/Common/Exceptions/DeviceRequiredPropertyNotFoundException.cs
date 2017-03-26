// <copyright file="DeviceRequiredPropertyNotFoundException.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace Common.Exceptions
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using Common.Helpers;

    [Serializable]
    public class DeviceRequiredPropertyNotFoundException : Exception
    {
        public DeviceRequiredPropertyNotFoundException(string message) : base(message)
        {
        }

        public DeviceRequiredPropertyNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        protected DeviceRequiredPropertyNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            EFGuard.NotNull(info, nameof(info));

            base.GetObjectData(info, context);
        }
    }
}
