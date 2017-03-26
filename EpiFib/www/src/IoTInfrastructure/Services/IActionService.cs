// <copyright file="IActionService.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace IoTInfrastructure.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IActionService
    {
        Task<List<string>> GetAllActionIdsAsync();

        Task<bool> ExecuteLogicAppAsync(string actionId, string deviceId, string measurementName, double measuredValue);
    }
}
