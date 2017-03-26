// <copyright file="EmergencyInstanceRequest.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace Common.Models
{
    using System;

    public class EmergencyInstanceRequest : IEmergencyBase
    {
        public string EmergencyInstanceId { get; set; }

        public EmergencyType EmergencyType { get; set; }

        public ExactLocation OwnerLocation { get; set; }

        public DateTime UpdatedTime { get; set; }

        public DateTime CreatedTime { get; set; }
    }
}
