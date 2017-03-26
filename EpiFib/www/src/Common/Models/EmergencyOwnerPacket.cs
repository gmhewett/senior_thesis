// <copyright file="EmergencyOwnerPacket.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace Common.Models
{
    using System;
    using System.Collections.Generic;

    public class EmergencyOwnerPacket : IEmergencyOwnerPacket
    {
        public string EmergencyInstanceId { get; set; }

        public EmergencyType EmergencyType { get; set; }

        public ExactLocation OwnerLocation { get; set; }

        public DateTime UpdatedTime { get; set; }

        public DateTime CreatedTime { get; set; }

        public string OwnerId { get; set; }

        public IEnumerable<EmergencyContainer> NearbyContainers { get; set; }

        public int NumUsersNotified { get; set; }

        public EmergencyUserInfo ResponderInfo { get; set; }

        public static explicit operator EmergencyOwnerPacket(EmergencyInstance v)
        {
            return new EmergencyOwnerPacket
            {
                EmergencyInstanceId = v.EmergencyInstanceId,
                EmergencyType = v.EmergencyType,
                NearbyContainers = v.NearbyContainers,
                NumUsersNotified = v.NumUsersNotified,
                OwnerId = v.OwnerId,
                OwnerLocation = v.OwnerLocation,
                ResponderInfo = v.ResponderInfo
            };
        }
    }
}
