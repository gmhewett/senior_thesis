// <copyright file="IDeviceTypeRepository.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace IoTInfrastructure.Repository
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using IoTInfrastructure.Models;

    public interface IDeviceTypeRepository
    {
        Task<List<DeviceType>> GetAllDeviceTypesAsync();

        Task<DeviceType> GetDeviceTypeAsync(int deviceTypeId);
    }
}
