// <copyright file="EmergencyInstance.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace Common.Models
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    public class EmergencyInstance : IEmergencyInstance
    {
        [SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Needed by DocDb")]
        public string id { get; set; }

        public string _rid { get; set; }

        public string _self { get; set; }

        public string _etag { get; set; }

        public int _ts { get; set; }

        public string _attachments { get; set; }

        public string EmergencyInstanceId { get; set; }

        public EmergencyType EmergencyType { get; set; }

        public ExactLocation OwnerLocation { get; set; }

        public DateTime UpdatedTime { get; set; }

        public DateTime CreatedTime { get; set; }

        public string OwnerId { get; set; }

        public IEnumerable<EmergencyContainer> NearbyContainers { get; set; }

        public int NumUsersNotified => this.NearbyContainers.Count();

        public EmergencyUserInfo ResponderInfo { get; set; }

        public EmergencyUserInfo OwnerInfo { get; set; }

        public IEnumerable<string> UserIdsNotified { get; set; }
    }
}
