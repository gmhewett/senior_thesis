// <copyright file="Command.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace Common.Models
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class Command
    {
        [JsonConstructor]
        public Command()
        {
            this.Parameters = new List<Parameter>();
        }

        public Command(string name, IEnumerable<Parameter> parameters = null) : this()
        {
            this.Name = name;
            if (parameters != null)
            {
                this.Parameters.AddRange(parameters);
            }
        }

        public string Name { get; set; }

        public List<Parameter> Parameters { get; set; }
    }
}
