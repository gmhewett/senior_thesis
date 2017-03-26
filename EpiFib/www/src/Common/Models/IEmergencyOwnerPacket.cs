// <copyright file="IEmergencyOwnerPacket.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace Common.Models
{
    using System.Collections.Generic;

    public interface IEmergencyOwnerPacket : IEmergencyBase
    {
        string OwnerId { get; set; }

        IEnumerable<EmergencyContainer> NearbyContainers { get; set; }

        int NumUsersNotified { get; }

        EmergencyUserInfo ResponderInfo { get; set; }
    }
}
