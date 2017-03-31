// <copyright file="ApnsNotificationPayload.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace PeerInfrastructure.Models
{
    using System.Diagnostics.CodeAnalysis;

    public class ApnsNotificationPayload
    {
        [SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Needed by DocDb")]

        public string alert { get; set; }

        [SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Needed by DocDb")]

        public string badge { get; set; }
    }
}
