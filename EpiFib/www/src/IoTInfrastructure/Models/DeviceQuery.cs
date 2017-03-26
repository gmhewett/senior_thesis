// <copyright file="DeviceQuery.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace IoTInfrastructure.Models
{
    using System.Collections.Generic;

    public class DeviceQuery
    {
        public List<FilterInfo> Filters { get; set; }

        public string SearchQuery { get; set; }

        public string SortColumn { get; set; }

        public QuerySortOrder SortOrder { get; set; }

        public int Skip { get; set; }
        
        public int Take { get; set; }
    }
}
