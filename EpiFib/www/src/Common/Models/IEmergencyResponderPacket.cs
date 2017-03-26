// <copyright file="IEmergencyResponderPacket.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace Common.Models
{
    public interface IEmergencyResponderPacket : IEmergencyBase
    {
        EmergencyUserInfo OwnerInfo { get; set; }
    }
}
