// <copyright file="Telemetry.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace Common.Models
{
    using Newtonsoft.Json;

    public class Telemetry
    {
        [JsonConstructor]
        public Telemetry()
        {
        }

        public Telemetry(string name, string displayName, string type)
        {
            this.Name = name;
            this.DisplayName = displayName;
            this.Type = type;
        }

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string Type { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
