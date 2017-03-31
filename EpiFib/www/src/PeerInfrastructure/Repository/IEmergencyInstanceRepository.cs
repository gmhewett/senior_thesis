// <copyright file="IEmergencyInstanceRepository.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace PeerInfrastructure.Repository
{
    using System;
    using System.Threading.Tasks;
    using Common.Models;

    public interface IEmergencyInstanceRepository
    {
        Task<EmergencyInstance> GetEmergencyInstanceAsync(string id, bool isDocDbId = true);

        Task<EmergencyInstance> GetEmergencyInstnaceWithLambda(Func<EmergencyInstance, bool> lambda);

        Task<EmergencyInstance> CreateEmergencyInstanceAsync(EmergencyInstance emergencyInstance);

        Task<EmergencyInstance> UpdateEmergencyInstanceAsync(EmergencyInstance emergencyInstance);

        Task DeleteEmergencyInstanceAsync(string id);
    }
}
