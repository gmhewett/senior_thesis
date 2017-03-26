// <copyright file="IDeviceRegistryListRepository.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace IoTInfrastructure.Repository
{
    using System.Threading.Tasks;
    using IoTInfrastructure.Models;

    public interface IDeviceRegistryListRepository
    {
        Task<DeviceListQueryResult> GetDeviceList(DeviceListQuery query);
    }
}
