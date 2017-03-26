// <copyright file="FilterHelper.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace IoTInfrastructure.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common.Helpers;
    using Common.Models;
    using IoTInfrastructure.Models;

    public class FilterHelper
    {
        public static IQueryable<DeviceModel> FilterDeviceList(
            IQueryable<DeviceModel> list,
            List<FilterInfo> filters)
        {
            EFGuard.NotNull(list, nameof(list));

            if (filters == null)
            {
                return list;
            }

            list = list.Where(GetIsNotNull).AsQueryable();

            foreach (FilterInfo f in filters)
            {
                if (!string.IsNullOrEmpty(f?.ColumnName))
                {
                    list = FilterItems(list, f);
                }
            }

            return list;
        }

        private static IQueryable<DeviceModel> FilterItems(
            IQueryable<DeviceModel> list,
            FilterInfo filter)
        {
            EFGuard.NotNull(list, nameof(list));
            EFGuard.NotNull(list, nameof(filter));
            EFGuard.NotNull(filter.ColumnName, nameof(filter.ColumnName));

            Func<DeviceProperties, dynamic> getValue = ReflectionHelper.ProducePropertyValueExtractor(
                    filter.ColumnName,
                    false,
                    false);

            Func<DeviceModel, bool> applyFilter = (item) =>
            {
                if (item == null)
                {
                    throw new ArgumentNullException(nameof(item));
                }

                if ((filter.FilterType == FilterType.Status) ||
                    string.Equals(
                        filter.ColumnName,
                        "Status",
                        StringComparison.CurrentCultureIgnoreCase))
                {
                    return GetValueMatchesStatus(item, filter.FilterValue);
                }

                if (item.DeviceProperties == null)
                {
                    return false;
                }

                dynamic columnValue = getValue(item.DeviceProperties);
                return GetValueSatisfiesFilter(columnValue, filter);
            };

            return list.Where(applyFilter).AsQueryable();
        }

        private static bool GetIsNotNull(dynamic item)
        {
            return item != null;
        }

        private static bool GetValueMatchesStatus(DeviceModel item, string statusName)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (string.IsNullOrEmpty(statusName))
            {
                return false;
            }

            string normalizedStatus = statusName.ToUpperInvariant();
            bool? enabledState = item.DeviceProperties?.HubEnabledState == null ? (bool?)null : item.DeviceProperties.GetHubEnabledState();

            switch (normalizedStatus)
            {
                case "RUNNING":
                    return enabledState == true;

                case "DISABLED":
                    return enabledState == false;

                case "PENDING":
                    return !enabledState.HasValue;

                default:
                    throw new ArgumentOutOfRangeException(nameof(statusName), statusName, "statusName has an unhandled status value.");
            }
        }

        private static bool GetValueSatisfiesFilter(
            dynamic value,
            FilterInfo filterInfo)
        {
            string strVal;

            if (value == null)
            {
                strVal = string.Empty;
            }
            else
            {
                strVal = value.ToString();
            }

            string match = filterInfo.FilterValue ?? string.Empty;

            switch (filterInfo.FilterType)
            {
                case FilterType.ContainsCaseInsensitive:
                    return strVal.IndexOf(match, StringComparison.CurrentCultureIgnoreCase) >= 0;

                case FilterType.ContainsCaseSensitive:
                    return strVal.IndexOf(match, StringComparison.CurrentCulture) >= 0;

                case FilterType.ExactMatchCaseInsensitive:
                    return string.Equals(strVal, match, StringComparison.CurrentCultureIgnoreCase);

                case FilterType.ExactMatchCaseSensitive:
                    return string.Equals(strVal, match, StringComparison.CurrentCulture);

                case FilterType.StartsWithCaseInsensitive:
                    return strVal.StartsWith(match, StringComparison.CurrentCultureIgnoreCase);

                case FilterType.StartsWithCaseSensitive:
                    return strVal.StartsWith(match, StringComparison.CurrentCulture);
            }

            return false;
        }
    }
}
