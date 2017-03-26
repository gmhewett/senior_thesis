// <copyright file="EmergencyInstanceService.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace PeerInfrastructure.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Common.Helpers;
    using Common.Models;
    using PeerInfrastructure.Repository;

    public class EmergencyInstanceService : IEmergencyInstanceService
    {
        private readonly IEmergencyInstanceRepository emergencyInstanceRepository;
        private readonly IUserLocationService userLocationService;

        public EmergencyInstanceService(
            IEmergencyInstanceRepository emergencyInstanceRepository, 
            IUserLocationService userLocationService)
        {
            EFGuard.NotNull(emergencyInstanceRepository, nameof(emergencyInstanceRepository));
            EFGuard.NotNull(userLocationService, nameof(userLocationService));

            this.emergencyInstanceRepository = emergencyInstanceRepository;
            this.userLocationService = userLocationService;
        }

        public async Task<EmergencyInstance> GetEmergencyInstnaceAsync(string id)
        {
            EFGuard.StringNotNull(id, nameof(id));

            return await this.emergencyInstanceRepository.GetEmergencyInstanceAsync(id);
        }

        public async Task<EmergencyOwnerPacket> GetExistingOwnerPacketForUser(string userId)
        {
            EFGuard.StringNotNull(userId, nameof(userId));

            EmergencyInstance emergencyInstance = await this.emergencyInstanceRepository.GetEmergencyInstnaceWithLambda(e => e.OwnerId == userId);
            return emergencyInstance != null ? (EmergencyOwnerPacket)emergencyInstance : null;
        }

        public async Task<EmergencyOwnerPacket> CreateEmergencyInstanceAsync(
            string ownerId,
            EmergencyInstanceRequest emergencyInstanceRequest)
        {
            EFGuard.NotNull(emergencyInstanceRequest, nameof(emergencyInstanceRequest));

            if (!UserLocationService.VerifyLatitudeAndLogitude(emergencyInstanceRequest.OwnerLocation))
            {
                return null;
            }

            if (emergencyInstanceRequest.EmergencyInstanceId != null)
            {
                EmergencyInstance existingInstance =
                    await this.emergencyInstanceRepository.GetEmergencyInstanceAsync(
                        emergencyInstanceRequest.EmergencyInstanceId);

                if (existingInstance != null)
                {
                    return null;
                }
            }

            EmergencyInstance newInstance = await this.ActOnEmergency(emergencyInstanceRequest, ownerId);

            return (EmergencyOwnerPacket)await this.emergencyInstanceRepository.CreateEmergencyInstanceAsync(newInstance);
        }

        public Task<EmergencyResponderPacket> RespondToEmergencyInstanceAsync(EmergencyUserInfo responderInfo)
        {
            throw new System.NotImplementedException();
        }

        public Task<EmergencyOwnerPacket> UpdateEmergencyOwnerInfoAsync(EmergencyUserInfo ownerInfo)
        {
            throw new System.NotImplementedException();
        }

        public Task<EmergencyResponderPacket> UpdateEmergencyResponderInfoAsync(EmergencyUserInfo responderInfo)
        {
            throw new System.NotImplementedException();
        }

        public Task DeleteEmergencyInstanceAsync(string id)
        {
            throw new System.NotImplementedException();
        }

        private Task<IEnumerable<EmergencyContainer>> GetNearbyContainers(ExactLocation location)
        {
            return Task.FromResult<IEnumerable<EmergencyContainer>>(new List<EmergencyContainer>());
        }

        private async Task<IEnumerable<string>> PingNearbyUsers(EmergencyInstanceRequest instanceRequest)
        {
            IEnumerable<string> hashedIds = await this.userLocationService.GetHashedUserIdsNearLocation(instanceRequest.OwnerLocation);
            return new List<string>();
        }

        private async Task<EmergencyInstance> ActOnEmergency(EmergencyInstanceRequest emergencyInstanceRequest, string ownerId)
        {
            switch (emergencyInstanceRequest.EmergencyType)
            {
                case EmergencyType.Autoinjector:
                    emergencyInstanceRequest.EmergencyInstanceId = Guid.NewGuid().ToString();

                    IEnumerable<EmergencyContainer> nearbyContainers =
                        await this.GetNearbyContainers(emergencyInstanceRequest.OwnerLocation);

                    IEnumerable<string> userIdsNotified = await this.PingNearbyUsers(emergencyInstanceRequest);

                    return new EmergencyInstance
                    {
                        EmergencyInstanceId = Guid.NewGuid().ToString(),
                        EmergencyType = emergencyInstanceRequest.EmergencyType,
                        NearbyContainers = nearbyContainers,
                        OwnerLocation = emergencyInstanceRequest.OwnerLocation,
                        OwnerId = ownerId,
                        UserIdsNotified = userIdsNotified,

                        //// TODO: finish these
                    };
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
