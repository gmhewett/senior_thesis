// <copyright file="IEpiFibDbContext.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace PeerInfrastructure.Repository
{
    using System.Data.Entity;
    using PeerInfrastructure.Models;

    public interface IEpiFibDbContext
    {
        IDbSet<EpiFibUser> Users { get; set; }
    }
}