// <copyright file="DeviceRuleDataFields.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace IoTInfrastructure.Models
{
    using System.Collections.Generic;

    public static class DeviceRuleDataFields
    {
        private static readonly List<string> AvailableDataFields = new List<string>
        {
            Temperature, Humidity
        };

        public static string Temperature => "Temperature";

        public static string Humidity => "Humidity";

        public static List<string> GetListOfAvailableDataFields()
        {
            return AvailableDataFields;
        }
    }
}
