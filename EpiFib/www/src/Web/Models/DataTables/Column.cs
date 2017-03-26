// <copyright file="Column.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace Web.Models.DataTables
{
    public class Column
    {
        public string Data { get; set; }

        public string Name { get; set; }

        public string Searchable { get; set; }

        public string Orderable { get; set; }

        public Search Search { get; set; }
    }
}