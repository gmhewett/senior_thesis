// <copyright file="EmergencyApiController.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace Web.ApiControllers
{
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Common.Helpers;
    using Common.Models;
    using Microsoft.AspNet.Identity;
    using PeerInfrastructure.Services;

    [Authorize]
    [RoutePrefix("api/v1/emergency")]
    public class EmergencyApiController : ApiControllerBase
    {
        private readonly IEmergencyInstanceService emergencyInstanceService;
        private readonly INotificationService notificationService;

        public EmergencyApiController(
            IEmergencyInstanceService emergencyInstanceService,
            INotificationService notificationService)
        {
            EFGuard.NotNull(emergencyInstanceService, nameof(emergencyInstanceService));
            EFGuard.NotNull(notificationService, nameof(notificationService));

            this.emergencyInstanceService = emergencyInstanceService;
            this.notificationService = notificationService;
        }

        [HttpPost]
        [Route("create")]
        public async Task<EmergencyOwnerPacket> CreateEmergencyInstance(EmergencyInstanceRequest emergencyRequest)
        {
            var existingInstance = await this.emergencyInstanceService.GetExistingOwnerPacketForUser(User.Identity.GetUserId());

            return existingInstance ??
                   await this.emergencyInstanceService.CreateEmergencyInstanceAsync(
                       User.Identity.GetUserId(),
                       emergencyRequest);
        }

        [HttpGet]
        [Route("current")]
        public async Task<HttpResponseMessage> GetCurrentEmergencyInstance()
        {
            if (string.IsNullOrWhiteSpace(User.Identity.GetUserId()))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "No User Id found.");
            }

            var ownerPacket = await this.emergencyInstanceService.GetExistingOwnerPacketForUser(User.Identity.GetUserId());

            return ownerPacket != null
                ? Request.CreateResponse(HttpStatusCode.OK, ownerPacket)
                : Request.CreateResponse(HttpStatusCode.OK, "No current packet.");
        }

        [HttpGet]
        [Route("test")]
        public async Task<HttpResponseMessage> Test()
        {
            await this.notificationService.Test();
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}