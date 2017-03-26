// <copyright file="Parameter.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace Common.Models
{
    using Newtonsoft.Json;

    public class Parameter
    {
        [JsonConstructor]
        public Parameter()
        {
        }

        public Parameter(string name, string type)
        {
            this.Name = name;
            this.Type = type;
        }

        public string Name { get; set; }

        public string Type { get; set; }
    }
}
