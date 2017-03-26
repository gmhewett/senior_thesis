// <copyright file="Configuration.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace PeerInfrastructure.Migrations
{
    using System.Data.Entity.Migrations;
    using Microsoft.AspNet.Identity;
    using PeerInfrastructure.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<Repository.EpiFibDbContext>
    {
        public Configuration()
        {
            this.AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(Repository.EpiFibDbContext context)
        {
            var passwordHash = new PasswordHasher();
            string password = passwordHash.HashPassword("Password@123");

            var user1 = new EpiFibUser
            {
                Id = "1",
                UserName = "json_bourne",
                PasswordHash = password,
                SecurityStamp = "stamp",
                Email = "json@treadstone.com"
            };

            context.Users.AddOrUpdate(x => x.UserName, user1);
        }
    }
}
