// <copyright file="UserLocationApiController.cs" company="The Reach Lab, LLC">
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
    [RoutePrefix("api/v1/userlocation")]
    public class UserLocationApiController : ApiControllerBase
    {
        private readonly IUserLocationService userLocationService;

        public UserLocationApiController(IUserLocationService userLocationService)
        {
            EFGuard.NotNull(userLocationService, nameof(userLocationService));

            this.userLocationService = userLocationService;
        }

        [Route("update")]
        public async Task<HttpResponseMessage> UpdateUserLocation(ExactLocation exactLocation)
        {
            UserLocation result = await this.userLocationService.UpdateUserLocationAsync(User.Identity.GetUserId(), exactLocation);

            return result == null
                ? new HttpResponseMessage(HttpStatusCode.BadRequest)
                : new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}