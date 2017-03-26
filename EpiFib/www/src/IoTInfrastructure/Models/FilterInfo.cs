// <copyright file="FilterInfo.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace IoTInfrastructure.Models
{
    public class FilterInfo
    {
        public string ColumnName { get; set; }

        public FilterType FilterType { get; set; }

        public string FilterValue { get; set; }
    }
}
