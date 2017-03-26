// <copyright file="UserLocationService.cs" company="The Reach Lab, LLC">
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
    using PeerInfrastructure.Models;
    using PeerInfrastructure.Repository;

    public class UserLocationService : IUserLocationService
    {
        private const int REarth = 6371;
        private const double Alpha = 1.4;
        private const double Beta = 1.4;

        private readonly IUserLocationRepository userLocationRepository;
        private readonly IHashService hashService;

        public UserLocationService(IUserLocationRepository userLocationRepository, IHashService hashService)
        {
            EFGuard.NotNull(userLocationRepository, nameof(userLocationRepository));
            EFGuard.NotNull(hashService, nameof(hashService));

            this.userLocationRepository = userLocationRepository;
            this.hashService = hashService;
        }

        public static bool VerifyLatitudeAndLogitude(ExactLocation location)
        {
            return location.Latitude > -90 && location.Latitude < 90 && 
                   location.Longitude > -180 && location.Longitude < 180;
        }

        public async Task<UserLocation> UpdateUserLocationAsync(string userId, ExactLocation location)
        {
            UserLocation cloakedLocation = this.CloakUserLocation(userId, location);
            if (cloakedLocation == null)
            {
                return null;
            }

            return await this.userLocationRepository.AddOrUpdateUserLocationAsync(cloakedLocation);
        }

        public async Task<IEnumerable<string>> GetHashedUserIdsNearLocation(ExactLocation location)
        {
            CloakedLocation cloakedLocation = CloakLocation(location);
            if (cloakedLocation == null)
            {
                return null;
            }

            return await this.userLocationRepository.GetHashedUserIdsNearLocationAsync(cloakedLocation);
        }

        private static CloakedLocation CloakLocation(ExactLocation location)
        {
            if (!VerifyLatitudeAndLogitude(location))
            {
                return null;
            }

            return new CloakedLocation
            {
                XPosition = CloackLongitude(location.Longitude),
                YPosition = CloakLatitude(location.Latitude)
            };
        }

        private static int CloakLatitude(double latitude)
        {
            double operand = Math.PI / 4 + ToRadians(latitude) / 2;
            double num = REarth * Math.Log(Math.Tan(operand)) / Beta;
            return num > 0 ? (int)Math.Floor(num) : (int)Math.Ceiling(num);
        }

        private static int CloackLongitude(double longitude)
        {
            double num = REarth * ToRadians(longitude) / Alpha;
            return num > 0 ? (int)Math.Floor(num) : (int)Math.Ceiling(num);
        }

        private static double ToRadians(double degrees)
        {
            return Math.PI * degrees / 180;
        }

        private UserLocation CloakUserLocation(string userId, ExactLocation location)
        {
            if (!VerifyLatitudeAndLogitude(location))
            {
                return null;
            }

            return new UserLocation
            {
                HashedUserId = this.hashService.HashString(userId),
                XPosition = CloackLongitude(location.Longitude),
                YPosition = CloakLatitude(location.Latitude)
            };
        }
    }
}
