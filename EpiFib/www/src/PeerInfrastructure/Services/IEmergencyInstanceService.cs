// <copyright file="IEmergencyInstanceService.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace PeerInfrastructure.Services
{
    using System.Threading.Tasks;
    using Common.Models;

    public interface IEmergencyInstanceService
    {
        Task<EmergencyInstance> GetEmergencyInstnaceAsync(string id);

        Task<EmergencyOwnerPacket> GetExistingOwnerPacketForUser(string userId);

        Task<EmergencyOwnerPacket> CreateEmergencyInstanceAsync(string ownerId, EmergencyInstanceRequest emergencyInstanceRequest);

        Task<EmergencyResponderPacket> RespondToEmergencyInstanceAsync(EmergencyUserInfo responderInfo);

        Task<EmergencyOwnerPacket> UpdateEmergencyOwnerInfoAsync(EmergencyUserInfo ownerInfo);

        Task<EmergencyResponderPacket> UpdateEmergencyResponderInfoAsync(EmergencyUserInfo responderInfo);

        Task DeleteEmergencyInstanceAsync(string id);
    }
}
