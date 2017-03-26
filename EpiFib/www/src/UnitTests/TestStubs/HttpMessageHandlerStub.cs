// <copyright file="HttpMessageHandlerStub.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace UnitTests.TestStubs
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using UnitTests.Infrastructure;

    public class HttpMessageHandlerStub : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, 
            CancellationToken cancellationToken)
        {
            if (string.Equals(
                request.RequestUri.ToString(), 
                ActionServiceTests.ENDPOINT,
                StringComparison.InvariantCultureIgnoreCase))
            {
                var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("Successful action")
                };
                return await Task.FromResult(responseMessage);
            }
            else
            {
                var responseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("Abort")
                };
                return await Task.FromResult(responseMessage);
            }
        }
    }
}
