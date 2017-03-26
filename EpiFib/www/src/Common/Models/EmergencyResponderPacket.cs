// <copyright file="EmergencyResponderPacket.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace Common.Models
{
    using System;

    public class EmergencyResponderPacket : IEmergencyResponderPacket
    {
        public string EmergencyInstanceId { get; set; }

        public EmergencyType EmergencyType { get; set; }

        public ExactLocation OwnerLocation { get; set; }

        public DateTime UpdatedTime { get; set; }

        public DateTime CreatedTime { get; set; }

        public EmergencyUserInfo OwnerInfo { get; set; }

        public static explicit operator EmergencyResponderPacket(EmergencyInstance v)
        {
            return new EmergencyResponderPacket
            {
                EmergencyInstanceId = v.EmergencyInstanceId,
                EmergencyType = v.EmergencyType,
                OwnerLocation = v.OwnerLocation,
                OwnerInfo = v.OwnerInfo
            };
        }
    }
}
