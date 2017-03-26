// <copyright file="IUserLocationRepository.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace PeerInfrastructure.Repository
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Common.Models;
    using PeerInfrastructure.Models;

    public interface IUserLocationRepository
    {
        Task<UserLocation> GetUserLocationAsync(string hashedUserId);

        Task<UserLocation> AddOrUpdateUserLocationAsync(UserLocation userLocation);

        Task DeleteUserLocationAsync(string hashedUserId);

        Task<IEnumerable<string>> GetHashedUserIdsNearLocationAsync(CloakedLocation userLocation, int xNear = 1, int yNear = 1);
    }
}
