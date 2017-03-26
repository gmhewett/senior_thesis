// <copyright file="DeviceTypeService.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace IoTInfrastructure.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using IoTInfrastructure.Models;
    using IoTInfrastructure.Repository;

    public class DeviceTypeService : IDeviceTypeService
    {
        private readonly IDeviceTypeRepository deviceTypeRepository;

        public DeviceTypeService(IDeviceTypeRepository deviceTypeRepository)
        {
            this.deviceTypeRepository = deviceTypeRepository;
        }

        public async Task<List<DeviceType>> GetAllDeviceTypesAsync()
        {
            return await this.deviceTypeRepository.GetAllDeviceTypesAsync();
        }

        public async Task<DeviceType> GetDeviceTypeAsync(int deviceTypeId)
        {
            return await this.deviceTypeRepository.GetDeviceTypeAsync(deviceTypeId);
        }
    }
}
