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
            if (!this.ModelState.IsValid)
            {
                return null;
            }

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

        [HttpPost]
        [Route("container")]
        public async Task<HttpResponseMessage> ToggleContainerAlarm(ContainerAlarmCommand containerAlarmCommand)
        {
            if (!this.ModelState.IsValid || string.IsNullOrWhiteSpace(containerAlarmCommand.EmergencyInstanceId) ||
                string.IsNullOrWhiteSpace(containerAlarmCommand.ContainerId))
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            return await this.emergencyInstanceService.ToggleContainerAlarm(containerAlarmCommand)
                ? this.Request.CreateResponse(HttpStatusCode.OK)
                : this.Request.CreateResponse(HttpStatusCode.BadRequest);
        }

        [HttpGet]
        [Route("nearby")]
        public async Task<HttpResponseMessage> GetEmergenciesNearby()
        {
            EmergencyInstanceRequest emergency = await this.emergencyInstanceService.GetEmergencyNearbyUser(User.Identity.GetUserId());

            return emergency != null
                ? Request.CreateResponse(HttpStatusCode.OK, emergency)
                : Request.CreateResponse(HttpStatusCode.OK, "No current packet.");
        }

        [HttpPost]
        [Route("respond")]
        public async Task<HttpResponseMessage> RespondToEmergency(EmergencyInstanceRequest request)
        {
            var responderPacket =
                (EmergencyResponderPacket)
                    await this.emergencyInstanceService.GetEmergencyInstnaceAsync(request.EmergencyInstanceId);

            return responderPacket != null
                ? Request.CreateResponse(HttpStatusCode.OK, responderPacket)
                : Request.CreateResponse(HttpStatusCode.BadRequest, "Could not respond.");
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