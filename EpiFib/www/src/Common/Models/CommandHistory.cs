// <copyright file="CommandHistory.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace Common.Models
{
    using System;
    using System.Collections.Generic;
    using Common.Helpers;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class CommandHistory
    {
        public CommandHistory(string name, dynamic parameters = null)
        {
            EFGuard.NotNull(name, nameof(name));

            this.Name = name;
            this.MessageId = Guid.NewGuid().ToString();
            this.CreatedTime = DateTime.UtcNow;
            this.SetParameters(parameters);
        }

        [JsonConstructor]
        internal CommandHistory()
        {
        }

        public string Name { get; set; }

        public string MessageId { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime UpdatedTime { get; set; }

        public string Result { get; set; }

        public string ErrorMessage { get; set; }

        public dynamic Parameters { get; set; }

        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }

        public void SetParameters(dynamic parameters)
        {
            if (parameters == null)
            {
                return;
            }

            if (parameters.GetType() == typeof(Dictionary<string, object>))
            {
                var newParam = new JObject();
                foreach (KeyValuePair<string, object> parameter in (Dictionary<string, object>)parameters)
                {
                    newParam.Add(parameter.Key, new JValue(parameter.Value));
                }

                this.Parameters = newParam;
            }
            else
            {
                this.Parameters = parameters;
            }
        }

        public string GetParameterString()
        {
            return this.Parameters == null ? string.Empty : this.Parameters.ToString();
        }
    }
}
