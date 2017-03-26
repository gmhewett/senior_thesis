// <copyright file="IVirtualDeviceStorage.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace Common.Repository
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Common.Models;

    public interface IVirtualDeviceStorage
    {
        Task<List<InitialDeviceConfig>> GetDeviceListAsync();

        Task<InitialDeviceConfig> GetDeviceAsync(string deviceId);

        Task AddOrUpdateDeviceAsync(InitialDeviceConfig deviceConfig);

        Task<bool> RemoveDeviceAsync(string deviceId);
    }
}
