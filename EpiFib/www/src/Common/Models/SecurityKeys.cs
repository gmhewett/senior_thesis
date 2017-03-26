// <copyright file="SecurityKeys.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace Common.Models
{
    using System.ComponentModel.DataAnnotations;

    public enum SecurityKey
    {
        None = 0,
        [Display(Name = "primary")]
        Primary,
        [Display(Name = "secondary")]
        Secondary
    }

    public class SecurityKeys
    {
        public SecurityKeys(string primaryKey, string secondaryKey)
        {
            this.PrimaryKey = primaryKey;
            this.SecondaryKey = secondaryKey;
        }

        public string PrimaryKey { get; set; }

        public string SecondaryKey { get; set; }
    }
}
