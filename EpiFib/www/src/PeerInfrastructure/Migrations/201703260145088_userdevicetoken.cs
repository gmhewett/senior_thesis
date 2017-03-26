// <copyright file="201703260145088_userdevicetoken.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace PeerInfrastructure.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using System.Diagnostics.CodeAnalysis;

    [SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Migration")]
    public partial class userdevicetoken : DbMigration
    {
        public override void Up()
        {
            this.AddColumn("dbo.AspNetUsers", "DeviceToken", c => c.String());
        }
        
        public override void Down()
        {
            this.DropColumn("dbo.AspNetUsers", "DeviceToken");
        }
    }
}
