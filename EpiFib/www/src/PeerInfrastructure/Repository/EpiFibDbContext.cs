// <copyright file="EpiFibDbContext.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace PeerInfrastructure.Repository
{
    using System.Data.Entity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using PeerInfrastructure.Migrations;
    using PeerInfrastructure.Models;

    public class EpiFibDbContext : IdentityDbContext<EpiFibUser>, IEpiFibDbContext
    {
        public EpiFibDbContext()
            : base("EpiFibUserDb", throwIfV1Schema: false)
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<EpiFibDbContext, Configuration>());
        }

        public static EpiFibDbContext Create()
        {
            return new EpiFibDbContext();
        }
    }
}