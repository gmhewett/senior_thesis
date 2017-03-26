// <copyright file="INotificationService.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace PeerInfrastructure.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Common.Models;

    public interface INotificationService
    {
        Task NotifyHashedUserIdsAsync(IEnumerable<string> hashedIds, EmergencyInstanceRequest instanceRequest);

        Task Test();
    }
}
