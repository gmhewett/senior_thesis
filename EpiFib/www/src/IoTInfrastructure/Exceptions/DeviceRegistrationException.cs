// <copyright file="DeviceRegistrationException.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace IoTInfrastructure.Exceptions
{
    using System;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using IoTInfrastructure.Properties;

    [Serializable]
    public class DeviceRegistrationException : DeviceAdministrationExceptionBase
    {
        public DeviceRegistrationException(string deviceId, Exception innerException) : base(deviceId, innerException)
        {
        }

        // protected constructor for deserialization
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        protected DeviceRegistrationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public override string Message => string.Format(
            CultureInfo.CurrentCulture,
            Resources.DeviceRegistrationExceptionMessage,
            this.DeviceId);
    }
}
