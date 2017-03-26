// <copyright file="DeviceTokenBindingModel.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace Web.Models.Account
{
    using System.ComponentModel.DataAnnotations;

    public class DeviceTokenBindingModel
    {
        [Required]
        [Display(Name = "DeviceToken")]
        public string DeviceToken { get; set; }
    }
}