// <copyright file="UnregisteredDeviceViewModel.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace Web.Models
{
    using System.ComponentModel.DataAnnotations;
    using IoTInfrastructure.Models;
    using Resources;

    public class UnregisteredDeviceViewModel
    {
        public const int MinimumDeviceIdLength = 2;

        public const int MaximumDeviceIdLength = 128;

        [Required]
        [StringLength(
            MaximumDeviceIdLength, 
            ErrorMessageResourceType = typeof(Strings), 
            ErrorMessageResourceName = "DeviceIDMustBeBetween2And128Characters", 
            MinimumLength = MinimumDeviceIdLength)]
        [RegularExpression("^[a-zA-Z0-9-_']+$",
            ErrorMessageResourceType = typeof(Strings),
            ErrorMessageResourceName = "DeviceIdContainsLettersNumbersHyphenUnderscore")]
        public string DeviceId { get; set; }

        [Required]
        public DeviceType DeviceType { get; set; }

        public bool IsDeviceIdSystemGenerated { get; set; }

        public bool IsDeviceIdUnique { get; set; }

        public string Iccid { get; set; }
    }
}