// <copyright file="DynamicValuesHelper.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace Common.Helpers
{
    using System;
    using System.Globalization;
    using Newtonsoft.Json;

    public static class DynamicValuesHelper
    {
        public static DateTime? ConvertToDateTime(dynamic value)
        {
            return ConvertToDateTime(CultureInfo.CurrentCulture, value);
        }
        
        public static DateTime? ConvertToDateTime(CultureInfo valueCultureInfo, dynamic value)
        {
            EFGuard.NotNull(valueCultureInfo, nameof(valueCultureInfo));

            DateTime dt = default(DateTime);
            if (value is DateTime)
            {
                return (DateTime)value;
            }

            return (value != null) &&
                   DateTime.TryParse(
                       value.ToString(),
                       valueCultureInfo,
                       DateTimeStyles.AllowWhiteSpaces,
                       out dt)
                ? (DateTime?)dt
                : null;
        }

        public static string ConvertToJsonString(dynamic value)
        {
            return value != null ? JsonConvert.SerializeObject(value) : null;
        }
    }
}
