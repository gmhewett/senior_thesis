// <copyright file="EmergencyInstanceRequest.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace Common.Models
{
    using System;

    public class EmergencyInstanceRequest : IEmergencyBase
    {
        public string EmergencyInstanceId { get; set; }

        public EmergencyType EmergencyType { get; set; }

        public ExactLocation OwnerLocation { get; set; }

        public EmergencyUserInfo OwnerInfo { get; set; }

        public DateTime UpdatedTime { get; set; }

        public DateTime CreatedTime { get; set; }

        public static explicit operator EmergencyInstanceRequest(EmergencyInstance v)
        {
            return new EmergencyInstanceRequest
            {
                EmergencyInstanceId = v.EmergencyInstanceId,
                EmergencyType = v.EmergencyType,
                OwnerLocation = v.OwnerLocation,
                UpdatedTime = v.UpdatedTime,
                CreatedTime = v.CreatedTime,
                OwnerInfo = v.OwnerInfo
            };
        }
    }
}
