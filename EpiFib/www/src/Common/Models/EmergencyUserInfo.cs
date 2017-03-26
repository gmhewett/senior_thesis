// <copyright file="EmergencyUserInfo.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace Common.Models
{
    public class EmergencyUserInfo
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Base64Pic { get; set; }

        public ExactLocation Location { get; set; }
    }
}
