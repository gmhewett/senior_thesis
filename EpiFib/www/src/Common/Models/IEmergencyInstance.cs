// <copyright file="IEmergencyInstance.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace Common.Models
{
    using System.Collections.Generic;

    public interface IEmergencyInstance : IEmergencyOwnerPacket, IEmergencyResponderPacket
    {
        IEnumerable<string> UserIdsNotified { get; set; }
    }
}
