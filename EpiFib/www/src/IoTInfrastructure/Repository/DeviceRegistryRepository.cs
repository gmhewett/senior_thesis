// <copyright file="DeviceRegistryRepository.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace IoTInfrastructure.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.Exceptions;
    using Common.Helpers;
    using Common.Models;
    using IoTInfrastructure.Exceptions;
    using IoTInfrastructure.Models;

    public class DeviceRegistryRepository : IDeviceRegistryCrudRepository, IDeviceRegistryListRepository
    {
        private readonly IDeviceDocumentDbClient<DeviceModel> documentClient;

        public DeviceRegistryRepository(IDeviceDocumentDbClient<DeviceModel> documentClient)
        {
            EFGuard.NotNull(documentClient, nameof(documentClient));

            this.documentClient = documentClient;
        }

        public async Task<DeviceModel> GetDeviceAsync(string deviceId)
        {
            EFGuard.StringNotNull(deviceId, nameof(deviceId));

            IQueryable<DeviceModel> query = await this.documentClient.QueryAsync();
            try
            {
                List<DeviceModel> devices = query.Where(x => x.DeviceProperties.DeviceID == deviceId).ToList();
                return devices.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not get device: {ex}");
            }
        }

        public async Task<DeviceModel> AddDeviceAsync(DeviceModel device)
        {
            EFGuard.NotNull(device, nameof(device));

            if (string.IsNullOrWhiteSpace(device.id))
            {
                device.id = Guid.NewGuid().ToString();
            }

            DeviceModel existingDevice = await this.GetDeviceAsync(device.DeviceProperties.DeviceID);
            if (existingDevice != null)
            {
                throw new DeviceAlreadyRegisteredException(device.DeviceProperties.DeviceID);
            }

            DeviceModel savedDevice = await this.documentClient.SaveAsync(device);
            return savedDevice;
        }

        public async Task<DeviceModel> UpdateDeviceAsync(DeviceModel device)
        {
            EFGuard.NotNull(device, nameof(device));

            if (device.DeviceProperties == null)
            {
                throw new DeviceRequiredPropertyNotFoundException(nameof(device.DeviceProperties) + " property is missing");
            }

            if (string.IsNullOrEmpty(device.DeviceProperties.DeviceID))
            {
                throw new DeviceRequiredPropertyNotFoundException(nameof(device.DeviceProperties.DeviceID) + " property is missing");
            }

            DeviceModel existingDevice = await this.GetDeviceAsync(device.DeviceProperties.DeviceID);
            if (existingDevice == null)
            {
                throw new DeviceNotRegisteredException(device.DeviceProperties.DeviceID);
            }

            string incomingRid = device._rid ?? string.Empty;

            if (string.IsNullOrWhiteSpace(incomingRid))
            {
                string existingRid = existingDevice._rid ?? string.Empty;
                if (string.IsNullOrWhiteSpace(existingRid))
                {
                    throw new InvalidOperationException("Could not find _rid property on existing device");
                }

                device._rid = existingRid;
            }

            string incomingId = device.id ?? string.Empty;

            if (string.IsNullOrWhiteSpace(incomingId))
            {
                if (existingDevice.DeviceProperties == null)
                {
                    throw new DeviceRequiredPropertyNotFoundException("'DeviceProperties' property is missing");
                }

                string existingId = existingDevice.id ?? string.Empty;
                if (string.IsNullOrWhiteSpace(existingId))
                {
                    throw new InvalidOperationException("Could not find id property on existing device");
                }

                device.id = existingId;
            }

            device.DeviceProperties.UpdatedTime = DateTime.UtcNow;
            DeviceModel savedDevice = await this.documentClient.SaveAsync(device);
            return savedDevice;
        }

        public async Task RemoveDeviceAsync(string deviceId)
        {
            EFGuard.StringNotNull(deviceId, nameof(deviceId));

            DeviceModel existingDevice = await this.GetDeviceAsync(deviceId);
            if (existingDevice == null)
            {
                throw new DeviceNotRegisteredException(deviceId);
            }

            await this.documentClient.DeleteAsync(existingDevice.id, existingDevice.DeviceProperties.DeviceID);
        }

        public async Task<DeviceModel> UpdateDeviceEnabledStatusAsync(string deviceId, bool isEnabled)
        {
            EFGuard.NotNull(deviceId, nameof(deviceId));

            DeviceModel existingDevice = await this.GetDeviceAsync(deviceId);

            if (existingDevice == null)
            {
                throw new DeviceNotRegisteredException(deviceId);
            }

            if (existingDevice.DeviceProperties == null)
            {
                throw new DeviceRequiredPropertyNotFoundException("Required DeviceProperties not found");
            }

            existingDevice.DeviceProperties.HubEnabledState = isEnabled;
            existingDevice.DeviceProperties.UpdatedTime = DateTime.UtcNow;
            DeviceModel savedDevice = await this.documentClient.SaveAsync(existingDevice);
            return savedDevice;
        }

        public async Task<DeviceListQueryResult> GetDeviceList(DeviceListQuery query)
        {
            List<DeviceModel> deviceList = await this.GetAllDevicesAsync();

            IQueryable<DeviceModel> filteredDevices = FilterHelper.FilterDeviceList(deviceList.AsQueryable<DeviceModel>(), query.Filters);

            IQueryable<DeviceModel> filteredAndSearchedDevices = this.SearchDeviceList(filteredDevices, query.SearchQuery);

            IQueryable<DeviceModel> sortedDevices = this.SortDeviceList(filteredAndSearchedDevices, query.SortColumn, query.SortOrder);

            List<DeviceModel> pagedDeviceList = sortedDevices.Skip(query.Skip).Take(query.Take).ToList();

            int filteredCount = filteredAndSearchedDevices.Count();

            return new DeviceListQueryResult()
            {
                Results = pagedDeviceList,
                TotalDeviceCount = deviceList.Count,
                TotalFilteredCount = filteredCount
            };
        }

        private async Task<List<DeviceModel>> GetAllDevicesAsync()
        {
            IQueryable<DeviceModel> devices = await this.documentClient.QueryAsync();
            return devices.ToList();
        }

        private IQueryable<DeviceModel> SearchDeviceList(IQueryable<DeviceModel> deviceList, string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return deviceList;
            }

            Func<DeviceModel, bool> filter = (d) => this.SearchTypePropertiesForValue(d, search);

            // look for all devices that contain the search value in one of the DeviceProperties Properties
            return deviceList.Where(filter).AsQueryable();
        }

        private bool SearchTypePropertiesForValue(DeviceModel device, string search)
        {
            // if the device or its system properties are null then
            // there's nothing that can be searched on
            if (device?.DeviceProperties == null)
            {
                return false;
            }

            // iterate through the DeviceProperties Properties and look for the search value
            // case insensitive search
            string upperCaseSearch = search.ToUpperInvariant();
            return device.DeviceProperties.ToKeyValuePairs().Any(t =>
                    (t.Value != null) &&
                    t.Value.ToString().ToUpperInvariant().Contains(upperCaseSearch));
        }

        private IQueryable<DeviceModel> SortDeviceList(IQueryable<DeviceModel> deviceList, string sortColumn, QuerySortOrder sortOrder)
        {
            // if a sort column was not provided then return the full device list in its original sort
            if (string.IsNullOrWhiteSpace(sortColumn))
            {
                return deviceList;
            }

            Func<DeviceProperties, dynamic> getPropVal = ReflectionHelper.ProducePropertyValueExtractor(sortColumn, false, false);
            Func<DeviceModel, dynamic> keySelector = (item) =>
            {
                if (item?.DeviceProperties == null)
                {
                    return null;
                }

                if (string.Equals("hubEnabledState", sortColumn, StringComparison.CurrentCultureIgnoreCase))
                {
                    return item.DeviceProperties.GetHubEnabledState();
                }

                return getPropVal(item.DeviceProperties);
            };

            if (sortOrder == QuerySortOrder.Ascending)
            {
                return deviceList.OrderBy(keySelector).AsQueryable();
            }
            else
            {
                return deviceList.OrderByDescending(keySelector).AsQueryable();
            }
        }
    }
}
