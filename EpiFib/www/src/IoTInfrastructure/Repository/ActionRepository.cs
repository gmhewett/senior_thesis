// <copyright file="ActionRepository.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace IoTInfrastructure.Repository
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    public class ActionRepository : IActionRepository
    {
        private readonly HttpMessageHandler handler;

        private readonly Dictionary<string, string> actionIds = new Dictionary<string, string>()
         {
            { "Send Message", string.Empty },
            { "Raise Alarm", string.Empty }
        };

        public ActionRepository(HttpMessageHandler handler = null)
        {
            this.handler = handler;
        }

        public async Task<bool> AddActionEndpoint(string actionId, string endpoint)
        {
            return await Task.Run(() =>
            {
                if (actionIds.ContainsKey(actionId) && !string.IsNullOrWhiteSpace(endpoint))
                {
                    actionIds[actionId] = endpoint;
                    return true;
                }

                return false;
            });
        }

        public async Task<List<string>> GetAllActionIdsAsync()
        {
            return await Task.Run(() => new List<string>(this.actionIds.Keys));
        }

        public async Task<bool> ExecuteLogicAppAsync(string actionId, string deviceId, string measurementName, double measuredValue)
        {
            if (this.actionIds.ContainsKey(actionId) && !string.IsNullOrEmpty(this.actionIds[actionId]))
            {
                return await Task.Run(async () =>
                {
                    using (var client = this.handler == null ? new HttpClient() : new HttpClient(this.handler))
                    {
                        var response = await client.PostAsync(
                            actionIds[actionId],
                            new StringContent(
                                JsonConvert.SerializeObject(
                                    new
                                    {
                                        deviceId,
                                        measurementName,
                                        measuredValue
                                    }),
                                 System.Text.Encoding.UTF8,
                                 "application/json"));
                        return response.IsSuccessStatusCode;
                    }
                });
            }

            Trace.TraceWarning($"ExecuteLogicAppAsync no event endpoint defined for actionId '{actionId}'");
            return false;
        }
    }
}
