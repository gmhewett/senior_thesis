// <copyright file="DeviceAlreadyRegisteredException.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace IoTInfrastructure.Exceptions
{
    using System;
    using System.Globalization;
    using System.Resources;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using IoTInfrastructure.Properties;

    [Serializable]
    public class DeviceAlreadyRegisteredException : DeviceAdministrationExceptionBase
    {
        public DeviceAlreadyRegisteredException(string deviceId) : base(deviceId)
        {
        }

        // protected constructor for deserialization
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        protected DeviceAlreadyRegisteredException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override string Message => string.Format(
            CultureInfo.CurrentCulture,
            Resources.DeviceAlreadyRegisteredExceptionMesage,
            this.DeviceId);
    }
}
