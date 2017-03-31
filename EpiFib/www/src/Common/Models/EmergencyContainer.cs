// <copyright file="EmergencyContainer.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace Common.Models
{
    using System.Collections.Generic;

    public class EmergencyContainer
    {
        public string DeviceId { get; set; }

        public IEnumerable<EmergencyType> EmergencyTypesSupported { get; set; }

        public ExactLocation Location { get; set; }
    }
}
