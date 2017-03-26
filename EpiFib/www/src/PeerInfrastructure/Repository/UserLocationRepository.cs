// <copyright file="UserLocationRepository.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace PeerInfrastructure.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.Helpers;
    using Common.Models;
    using PeerInfrastructure.Helpers;
    using PeerInfrastructure.Models;

    public class UserLocationRepository : IUserLocationRepository
    {
        private readonly IUserLocationDocumentDbClient<UserLocation> documentDbClient;

        public UserLocationRepository(IUserLocationDocumentDbClient<UserLocation> documentDbClient)
        {
            EFGuard.NotNull(documentDbClient, nameof(documentDbClient));

            this.documentDbClient = documentDbClient;
        }
        
        public async Task<UserLocation> GetUserLocationAsync(string hashedUserId)
        {
            EFGuard.NotNull(hashedUserId, nameof(hashedUserId));

            IQueryable<UserLocation> query = await this.documentDbClient.QueryAsync();
            try
            {
                IEnumerable<UserLocation> locations = query.Where(l => l.HashedUserId == hashedUserId).ToList();
                return locations.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not get user location: {ex}");
            }
        }

        public async Task<UserLocation> AddOrUpdateUserLocationAsync(UserLocation userLocation)
        {
            EFGuard.NotNull(userLocation, nameof(userLocation));

            if (string.IsNullOrWhiteSpace(userLocation.HashedUserId))
            {
                throw new Exception("No hashed user id found in UserLocation.");
            }

            userLocation.UpdatedTime = DateTime.UtcNow;
            UserLocation exisitingLocation = await this.GetUserLocationAsync(userLocation.HashedUserId);

            if (exisitingLocation == null)
            {
                UserLocation newLocation = await this.documentDbClient.SaveAsync(userLocation);
                return newLocation;
            }

            string incomingRid = userLocation._rid ?? string.Empty;

            if (string.IsNullOrWhiteSpace(incomingRid))
            {
                string existingRid = exisitingLocation._rid ?? string.Empty;
                if (string.IsNullOrWhiteSpace(existingRid))
                {
                    throw new InvalidOperationException("Could not find _rid property on existing location.");
                }

                userLocation._rid = existingRid;
            }

            string incomingId = userLocation.id ?? string.Empty;

            if (string.IsNullOrWhiteSpace(incomingId))
            {
                string existingId = exisitingLocation.id ?? string.Empty;

                if (string.IsNullOrWhiteSpace(existingId))
                {
                    throw new InvalidOperationException("Could not find id property on existing location.");
                }

                userLocation.id = existingId;
            }

            UserLocation udpatedLocation = await this.documentDbClient.SaveAsync(userLocation);
            return udpatedLocation;
        }

        public async Task DeleteUserLocationAsync(string hashedUserId)
        {
            EFGuard.StringNotNull(hashedUserId, nameof(hashedUserId));

            UserLocation existingLocation = await this.GetUserLocationAsync(hashedUserId);
            if (existingLocation == null)
            {
                throw new Exception("User Location not found.");
            }

            await this.documentDbClient.DeleteAsync(existingLocation.id, existingLocation.HashedUserId);
        }

        public async Task<IEnumerable<string>> GetHashedUserIdsNearLocationAsync(
            CloakedLocation cloakedLocation, 
            int xNear = 1, 
            int yNear = 1)
        {
            IQueryable<UserLocation> query = await this.documentDbClient.QueryAsync();
            try
            {
                return
                    (from location in query
                     let isXNear = Math.Abs(location.XPosition - cloakedLocation.XPosition) <= xNear
                     let isYNear = Math.Abs(location.YPosition - cloakedLocation.YPosition) <= yNear
                     where isXNear && isYNear
                     select location.HashedUserId).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not get users near location: {ex}");
            }
        }
    }
}
