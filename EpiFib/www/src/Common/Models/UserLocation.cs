// <copyright file="UserLocation.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace Common.Models
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    public class UserLocation
    {
        [SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Needed by DocDb")]
        public string id { get; set; }

        public string _rid { get; set; }

        public string _self { get; set; }

        public string _etag { get; set; }

        public int _ts { get; set; }

        public string _attachments { get; set; }

        public string HashedUserId { get; set; }

        public int XPosition { get; set; }

        public int YPosition { get; set; }

        public DateTime UpdatedTime { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
