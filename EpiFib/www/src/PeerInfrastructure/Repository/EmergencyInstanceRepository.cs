// <copyright file="EmergencyInstanceRepository.cs" company="The Reach Lab, LLC">
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

    public class EmergencyInstanceRepository : IEmergencyInstanceRepository
    {
        private readonly IEmergencyInstanceDocumentDbClient<EmergencyInstance> documentDbClient;

        public EmergencyInstanceRepository(IEmergencyInstanceDocumentDbClient<EmergencyInstance> documentDbClient)
        {
            EFGuard.NotNull(documentDbClient, nameof(documentDbClient));

            this.documentDbClient = documentDbClient;
        }

        public async Task<EmergencyInstance> GetEmergencyInstanceAsync(string id)
        {
            EFGuard.NotNull(id, nameof(id));

            IQueryable<EmergencyInstance> query = await this.documentDbClient.QueryAsync();
            try
            {
                IEnumerable<EmergencyInstance> locations = query.Where(e => e.id == id).ToList();
                return locations.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not get emergency instance: {ex}");
            }
        }

        public async Task<EmergencyInstance> GetEmergencyInstnaceWithLambda(Func<EmergencyInstance, bool> lambda)
        {
            EFGuard.NotNull(lambda, nameof(lambda));

            IQueryable<EmergencyInstance> query = await this.documentDbClient.QueryAsync();
            try
            {
                IEnumerable<EmergencyInstance> locations = query.Where(lambda).ToList();
                return locations.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not get emergency instance: {ex}");
            }
        }

        public async Task<EmergencyInstance> CreateEmergencyInstanceAsync(EmergencyInstance emergencyInstance)
        {
            EFGuard.NotNull(emergencyInstance, nameof(emergencyInstance));

            if (string.IsNullOrWhiteSpace(emergencyInstance.id))
            {
                emergencyInstance.id = Guid.NewGuid().ToString();
            }

            EmergencyInstance existingDevice = await this.GetEmergencyInstanceAsync(emergencyInstance.id);
            if (existingDevice != null)
            {
                throw new Exception("EmergencyInstance aleady created");
            }

            EmergencyInstance savedEmergencyInstance = await this.documentDbClient.SaveAsync(emergencyInstance);
            return savedEmergencyInstance;
        }

        public async Task<EmergencyInstance> UpdateEmergencyInstanceAsync(EmergencyInstance emergencyInstance)
        {
            EFGuard.NotNull(emergencyInstance, nameof(emergencyInstance));

            if (string.IsNullOrWhiteSpace(emergencyInstance.id))
            {
                throw new InvalidOperationException("Could not find id property on emergencyInstance.");
            }

            EmergencyInstance existingEmergencyInstance = await this.GetEmergencyInstanceAsync(emergencyInstance.id);
            if (existingEmergencyInstance == null)
            {
                throw new Exception("Emergency Instance not found.");
            }

            string incomingRid = emergencyInstance._rid ?? string.Empty;

            if (string.IsNullOrWhiteSpace(incomingRid))
            {
                string existingRid = existingEmergencyInstance._rid ?? string.Empty;
                if (string.IsNullOrWhiteSpace(existingRid))
                {
                    throw new InvalidOperationException("Could not find _rid property on existing emergencyInstance");
                }

                emergencyInstance._rid = existingRid;
            }

            string incomingId = emergencyInstance.id ?? string.Empty;

            if (string.IsNullOrWhiteSpace(incomingId))
            {
                string existingId = existingEmergencyInstance.id ?? string.Empty;
                if (string.IsNullOrWhiteSpace(existingId))
                {
                    throw new InvalidOperationException("Could not find id property on existing emergencyInstance");
                }

                emergencyInstance.id = existingId;
            }

            emergencyInstance.UpdatedTime = DateTime.UtcNow;
            EmergencyInstance savedEmergencyInstance = await this.documentDbClient.SaveAsync(emergencyInstance);
            return savedEmergencyInstance;
        }

        public async Task DeleteEmergencyInstanceAsync(string id)
        {
            EFGuard.StringNotNull(id, nameof(id));

            EmergencyInstance existingEmergencyInstance = await this.GetEmergencyInstanceAsync(id);
            if (existingEmergencyInstance == null)
            {
                throw new Exception("User Location not found.");
            }

            await this.documentDbClient.DeleteAsync(existingEmergencyInstance.id, existingEmergencyInstance.OwnerId);
        }
    }
}