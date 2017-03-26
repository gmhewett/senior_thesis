// <copyright file="IUserLocationService.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace PeerInfrastructure.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Common.Models;

    public interface IUserLocationService
    {
        Task<UserLocation> UpdateUserLocationAsync(string userId, ExactLocation location);

        Task<IEnumerable<string>> GetHashedUserIdsNearLocation(ExactLocation location);
    }
}
