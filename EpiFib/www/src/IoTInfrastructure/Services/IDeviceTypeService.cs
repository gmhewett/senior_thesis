// <copyright file="IDeviceTypeService.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace IoTInfrastructure.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using IoTInfrastructure.Models;

    public interface IDeviceTypeService
    {
        Task<List<DeviceType>> GetAllDeviceTypesAsync();

        Task<DeviceType> GetDeviceTypeAsync(int deviceTypeId);
    }
}
