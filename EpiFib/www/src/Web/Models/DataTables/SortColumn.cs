// <copyright file="SortColumn.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace Web.Models.DataTables
{
    using System.Globalization;
    using IoTInfrastructure.Models;
    using Newtonsoft.Json;

    public class SortColumn
    {
        [JsonProperty("column")]
        public string ColumnIndexAsString { get; set; }

        public int ColumnIndex => int.Parse(this.ColumnIndexAsString, NumberStyles.Integer, CultureInfo.CurrentCulture);
        
        public QuerySortOrder SortOrder => this.Direction == "asc" ? QuerySortOrder.Ascending : QuerySortOrder.Descending;

        [JsonProperty("dir")]
        private string Direction { get; set; }
    }
}