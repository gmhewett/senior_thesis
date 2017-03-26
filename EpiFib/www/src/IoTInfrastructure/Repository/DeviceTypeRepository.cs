// <copyright file="DeviceTypeRepository.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace IoTInfrastructure.Repository
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using IoTInfrastructure.Models;
    using IoTInfrastructure.Properties;

    public class DeviceTypeRepository : IDeviceTypeRepository
    {
        private readonly List<DeviceType> deviceTypes = new List<DeviceType>
        {
            new DeviceType
            {
                Name = Resources.SimulatedDeviceName,
                DeviceTypeId = 1,
                Description = Resources.SimulatedDeviceDescription,
                InstructionsUrl = null,
                IsSimulatedDevice = true
            },
            new DeviceType
            {
                Name = Resources.CustomDeviceName,
                DeviceTypeId = 2,
                Description = Resources.CustomDeviceDescription,
                InstructionsUrl = Resources.CustomDeviceInstructionsUrl
            }
        };

        public async Task<List<DeviceType>> GetAllDeviceTypesAsync()
        {
            return await Task.Run(() => this.deviceTypes);
        }

        public async Task<DeviceType> GetDeviceTypeAsync(int deviceTypeId)
        {
            return await Task.Run(() =>
            {
                return this.deviceTypes.FirstOrDefault(dt => dt.DeviceTypeId == deviceTypeId);
            });
        }
    }
}
