// <copyright file="ActionServiceTests.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace UnitTests.Infrastructure
{
    using System.Threading.Tasks;
    using global::IoTInfrastructure.Repository;
    using global::IoTInfrastructure.Services;
    using UnitTests.TestStubs;
    using Xunit;

    public class ActionServiceTests
    {
        private readonly IActionRepository actionRepository;
        private readonly ActionService actionService;

        public ActionServiceTests()
        {
            this.actionRepository = new ActionRepository(new HttpMessageHandlerStub());
            this.actionService = new ActionService(this.actionRepository);
        }

        public static string ENDPOINT => "http://www.Test.Endpoint/";

        [Fact]
        public async Task ExecuteLogicAppAsyncTest()
        {
            string actionId = "Send Message";
            string deviceId = "TestDeviceID";
            string measurementName = "TestMeasurementName";
            double measuredValue = 10.0;

            bool res = await this.actionService.ExecuteLogicAppAsync(actionId, deviceId, measurementName, measuredValue);
            Assert.False(res);

            await this.actionRepository.AddActionEndpoint(actionId, ENDPOINT);
            res = await this.actionService.ExecuteLogicAppAsync(actionId, deviceId, measurementName, measuredValue);
            Assert.True(res);
        }
    }
}
