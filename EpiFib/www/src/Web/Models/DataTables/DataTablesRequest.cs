// <copyright file="DataTablesRequest.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace Web.Models.DataTables
{
    using System.Collections.Generic;
    using IoTInfrastructure.Models;
    using Newtonsoft.Json;

    public class DataTablesRequest
    {
        public int Draw { get; set; }

        public int Start { get; set; }

        public int Length { get; set; }

        public List<Column> Columns { get; set; }

        [JsonProperty("order")]
        public List<SortColumn> SortColumns { get; set; }

        public Search Search { get; set; }

        public List<FilterInfo> Filters { get; set; }
    }
}