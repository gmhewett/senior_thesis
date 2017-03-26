// <copyright file="DeviceAdministrationExceptionBase.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace IoTInfrastructure.Exceptions
{
    using System;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using Common.Helpers;
    using IoTInfrastructure.Properties;

    [Serializable]
    public class DeviceAdministrationExceptionBase : Exception
    {
        public DeviceAdministrationExceptionBase()
        {
        }

        public DeviceAdministrationExceptionBase(string deviceId)
            : base(string.Format(CultureInfo.CurrentCulture, Resources.DeviceIdFormatString, deviceId))
        {
            this.DeviceId = deviceId;
        }

        public DeviceAdministrationExceptionBase(string deviceId, Exception innerException)
            : base(string.Format(CultureInfo.CurrentCulture, Resources.DeviceIdFormatString, deviceId), innerException)
        {
            this.DeviceId = deviceId;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        protected DeviceAdministrationExceptionBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            EFGuard.NotNull(info, nameof(info));

            this.DeviceId = info.GetString("DeviceId");
        }
        
        public string DeviceId { get; set; }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            EFGuard.NotNull(info, nameof(info));

            info.AddValue("DeviceId", this.DeviceId);
            base.GetObjectData(info, context);
        }
    }
}
