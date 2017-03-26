// <copyright file="ActionModel.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace IoTInfrastructure.Models
{
    using System;
    using Newtonsoft.Json;

    public class ActionModel
    {
        // column names in ASA job output
        private const string DeviceIdColumnName = "deviceid";
        private const string ReadingTypeColumnName = "readingtype";
        private const string ReadingValueColumnName = "reading";
        private const string ThresholdValueColumnName = "threshold";
        private const string RuleOutputColumnName = "ruleoutput";
        private const string TimeColumnName = "time";

        [JsonProperty(PropertyName = DeviceIdColumnName)]
        public string DeviceID { get; set; }

        [JsonProperty(PropertyName = ReadingTypeColumnName)]
        public string ReadingType { get; set; }

        [JsonProperty(PropertyName = ReadingValueColumnName)]
        public double Reading { get; set; }

        [JsonProperty(PropertyName = ThresholdValueColumnName)]
        public double Threshold { get; set; }

        [JsonProperty(PropertyName = RuleOutputColumnName)]
        public string RuleOutput { get; set; }

        [JsonProperty(PropertyName = TimeColumnName)]
        public DateTime Time { get; set; }
    }
}
