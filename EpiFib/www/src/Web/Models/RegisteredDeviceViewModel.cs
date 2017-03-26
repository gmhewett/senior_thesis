// <copyright file="RegisteredDeviceViewModel.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace Web.Models
{
    using System.ComponentModel.DataAnnotations;
    using Resources;

    public class RegisteredDeviceViewModel : UnregisteredDeviceViewModel
    {
        public const int MaxLength = 200;

        public string PrimaryKey { get; set; }

        public string SecondaryKey { get; set; }

        [StringLength(
            MaxLength, 
            ErrorMessageResourceType = typeof(Strings), 
            ErrorMessageResourceName = "ObjectNameMustBeLessThan200Characters")]
        public string ObjectName { get; set; }

        [StringLength(
            MaxLength, 
            ErrorMessageResourceType = typeof(Strings), 
            ErrorMessageResourceName = "VersionMustBeLessThan200Characters")]
        public string Version { get; set; }

        public string Manufacturer { get; set; }

        [StringLength(
            MaxLength, 
            ErrorMessageResourceType = typeof(Strings), 
            ErrorMessageResourceName = "ModelNameMustBeLessThan200Characters")]
        public string ModelNumber { get; set; }

        [StringLength(
            MaxLength, 
            ErrorMessageResourceType = typeof(Strings), 
            ErrorMessageResourceName = "SerialNumberMustBeLessThan200Characters")]
        public string SerialNumber { get; set; }

        [StringLength(
            MaxLength, 
            ErrorMessageResourceType = typeof(Strings), 
            ErrorMessageResourceName = "FirmwareVersionMustBeLessThan200Characters")]
        public string FirmwareVersion { get; set; }

        [StringLength(
            MaxLength, 
            ErrorMessageResourceType = typeof(Strings), 
            ErrorMessageResourceName = "PlatformNameMustBeLessThan200Characters")]
        public string Platform { get; set; }

        [StringLength(
            MaxLength, 
            ErrorMessageResourceType = typeof(Strings), 
            ErrorMessageResourceName = "ProcessorNameMustBeLessThan200Characters")]
        public string Processor { get; set; }

        [StringLength(
            MaxLength,
            ErrorMessageResourceType = typeof(Strings),
            ErrorMessageResourceName = "InstalledRAMFieldMustBeLessThan200Characters")]
        public string InstalledRAM { get; set; }

        public bool IsEdit { get; set; }

        public bool SupportsChangeConfigCommand { get; set; }

        public string HostName { get; set; }

        public string InstructionsUrl { get; set; }

        public string IsCellular { get; set; }
    }
}