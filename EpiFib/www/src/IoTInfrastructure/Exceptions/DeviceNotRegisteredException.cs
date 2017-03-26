// <copyright file="DeviceNotRegisteredException.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace IoTInfrastructure.Exceptions
{
    using System;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using IoTInfrastructure.Properties;

    [Serializable]
    public class DeviceNotRegisteredException : DeviceAdministrationExceptionBase
    {
        public DeviceNotRegisteredException(string deviceId) : base(deviceId)
        {
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        protected DeviceNotRegisteredException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public override string Message => string.Format(
            CultureInfo.CurrentCulture,
            Resources.DeviceNotRegisteredExceptionMessage,
            this.DeviceId);
    }
}
