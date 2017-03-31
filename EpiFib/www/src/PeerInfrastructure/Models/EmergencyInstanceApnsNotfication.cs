// <copyright file="EmergencyInstanceApnsNotfication.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace PeerInfrastructure.Models
{
    using System.Diagnostics.CodeAnalysis;
    using Common.Models;

    public class EmergencyInstanceApnsNotfication
    {
        [SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Needed by DocDb")]

        public ApnsNotificationPayload aps { get; set; }

        [SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Needed by DocDb")]

        public EmergencyInstanceRequest emergencyInstance { get; set; }
    }
}
