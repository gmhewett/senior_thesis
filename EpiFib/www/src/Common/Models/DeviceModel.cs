// <copyright file="DeviceModel.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace Common.Models
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    public class DeviceModel
    {
        public DeviceModel()
        {
            this.Commands = new List<Command>();
            this.CommandHistory = new List<CommandHistory>();
            this.Telemetry = new List<Telemetry>();
        }

        [SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Needed by DocDb")]
        public string id { get; set; }

        public string _rid { get; set; }

        public string _self { get; set; }

        public string _etag { get; set; }

        public int _ts { get; set; }

        public string _attachments { get; set; }

        public DeviceProperties DeviceProperties { get; set; }

        public SystemProperties SystemProperties { get; set; }

        public IoTHub IoTHub { get; set; }

        public List<Command> Commands { get; set; }

        public List<CommandHistory> CommandHistory { get; set; }

        public bool IsSimulatedDevice { get; set; }

        public List<Telemetry> Telemetry { get; set; }

        public string Version { get; set; }

        public string ObjectType { get; set; }

        public string ObjectName { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
