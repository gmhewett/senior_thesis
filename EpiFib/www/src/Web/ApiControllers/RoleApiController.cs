// <copyright file="RoleApiController.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace Web.ApiControllers
{
    using System;
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Microsoft.AspNet.Identity.Owin;
    using PeerInfrastructure.Repository;
    using Web.Models.Identity;
    using Web.Security;
    using Web.Services;

    [RoutePrefix("api/v1/roles")]
    public class RoleApiController : ApiController
    {
        private readonly EpiFibDbContext dbContext;

        public RoleApiController()
        {
            this.dbContext = new EpiFibDbContext();
        }

        [HttpPost]
        [Route("create")]
        public async Task<IHttpActionResult> CreateRole(IdentityRole role)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            this.dbContext.Roles.Add(role);

            string error;
            try
            {
                await this.dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                error = $"Could not create role due to DbUpdateException: {ex}";
                Trace.TraceError(error);
                return this.BadRequest(error);
            }
            catch (Exception ex)
            {
                error = $"Could not create role: {ex}";
                Trace.TraceError(error);
                return this.BadRequest(error);
            }

            return this.Created("/", role);
        }

        [HttpPost]
        [Route("AddUser")]
        public async Task<IHttpActionResult> AddUserRole(UserRoleBindingModel userRole)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            string validRole = Role.GetValidRoleName(userRole.RoleName);

            if (string.IsNullOrWhiteSpace(validRole))
            {
                return this.BadRequest();
            }

            var epiFibUser = this.dbContext.Users.FirstOrDefault(u => u.Id == userRole.UserId);

            if (epiFibUser == null)
            {
                return this.BadRequest();
            }

            var manager = Request.GetOwinContext().GetUserManager<EpiFibUserManager>();

            if (manager == null)
            {
                return this.BadRequest();
            }

            IdentityResult roleResult = await manager.AddToRoleAsync(epiFibUser.Id, validRole);

            if (roleResult.Succeeded)
            {
                return this.Ok();
            }

            return this.BadRequest();         
        }
    }
}