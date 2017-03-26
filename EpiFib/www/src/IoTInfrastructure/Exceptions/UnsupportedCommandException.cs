// <copyright file="UnsupportedCommandException.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace IoTInfrastructure.Exceptions
{
    using System;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using Common.Helpers;
    using IoTInfrastructure.Properties;

    [Serializable]
    public class UnsupportedCommandException : DeviceAdministrationExceptionBase
    {
        public UnsupportedCommandException(string deviceId, string commandName) : base(deviceId)
        {
            this.CommandName = commandName;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        protected UnsupportedCommandException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            EFGuard.NotNull(info, nameof(info));

            this.CommandName = info.GetString("CommandName");
        }

        public string CommandName { get; set; }

        public override string Message => string.Format(
            CultureInfo.CurrentCulture,
            Resources.UnsupportedCommandExceptionMessage,
            this.DeviceId,
            this.CommandName);

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            EFGuard.NotNull(info, nameof(info));

            info.AddValue("CommandName", this.CommandName);
            base.GetObjectData(info, context);
        }
    }
}
