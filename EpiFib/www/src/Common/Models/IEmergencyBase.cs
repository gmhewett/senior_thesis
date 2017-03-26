// <copyright file="IEmergencyBase.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace Common.Models
{
    using System;

    public interface IEmergencyBase
    {
        string EmergencyInstanceId { get; set; }

        EmergencyType EmergencyType { get; set; }

        ExactLocation OwnerLocation { get; set; }

        DateTime UpdatedTime { get; set; }

        DateTime CreatedTime { get; set; }
    }
}
