// <copyright file="EmergencyInstanceService.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace PeerInfrastructure.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.Helpers;
    using Common.Models;
    using IoTInfrastructure.Services;
    using PeerInfrastructure.Repository;

    public class EmergencyInstanceService : IEmergencyInstanceService
    {
        private readonly IEmergencyInstanceRepository emergencyInstanceRepository;
        private readonly IUserLocationService userLocationService;
        private readonly INotificationService notificationService;
        private readonly IDeviceService deviceService;

        public EmergencyInstanceService(
            IEmergencyInstanceRepository emergencyInstanceRepository, 
            IUserLocationService userLocationService,
            INotificationService notificationService,
            IDeviceService deviceService)
        {
            EFGuard.NotNull(emergencyInstanceRepository, nameof(emergencyInstanceRepository));
            EFGuard.NotNull(userLocationService, nameof(userLocationService));
            EFGuard.NotNull(notificationService, nameof(notificationService));
            EFGuard.NotNull(deviceService, nameof(deviceService));

            this.emergencyInstanceRepository = emergencyInstanceRepository;
            this.userLocationService = userLocationService;
            this.notificationService = notificationService;
            this.deviceService = deviceService;
        }

        public async Task<EmergencyInstance> GetEmergencyInstnaceAsync(string id, bool isDocDbId = true)
        {
            EFGuard.StringNotNull(id, nameof(id));

            return await this.emergencyInstanceRepository.GetEmergencyInstanceAsync(id, isDocDbId);
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

        public async Task<bool> ToggleContainerAlarm(ContainerAlarmCommand containerAlarmCommand)
        {
            EmergencyInstance existingEmergency =
                await this.GetEmergencyInstnaceAsync(containerAlarmCommand.EmergencyInstanceId, false);

            if (existingEmergency.NearbyContainers.All(c => c.DeviceId != containerAlarmCommand.ContainerId))
            {
                return false;
            }

            IDictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "type", containerAlarmCommand.Value }
            };

            await this.deviceService.SendCommandAsync(containerAlarmCommand.ContainerId, "ToggleAlarm", parameters);

            return true;
        }

        public async Task<EmergencyInstanceRequest> GetEmergencyNearbyUser(string userId)
        {
            var emergecnyNearby =
               await this.emergencyInstanceRepository.GetEmergencyInstnaceWithLambda(e => e.UserIdsNotified.Contains(userId));

            return emergecnyNearby == null ? null : (EmergencyInstanceRequest)emergecnyNearby;
        }

        private async Task<IEnumerable<EmergencyContainer>> GetNearbyContainers(ExactLocation location)
        {
            var devices = await this.deviceService.GetDevicesNear(location);

            return devices.Select(device => new EmergencyContainer
            {
                DeviceId = device.DeviceProperties.DeviceID,
                Location = new ExactLocation
                {
                    Latitude = device.DeviceProperties.Latitude ?? 0.0,
                    Longitude = device.DeviceProperties.Longitude ?? 0.0
                }
            }).ToList();
        }

        private async Task<IEnumerable<string>> PingNearbyUsers(EmergencyInstanceRequest instanceRequest)
        {
            IEnumerable<string> hashedIds = await this.userLocationService.GetHashedUserIdsNearLocation(instanceRequest.OwnerLocation);
            return await this.notificationService.NotifyHashedUserIdsAsync(hashedIds, instanceRequest);
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
