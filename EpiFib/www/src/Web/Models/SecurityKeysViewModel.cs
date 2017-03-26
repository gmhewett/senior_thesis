// <copyright file="SecurityKeysViewModel.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace Web.Models
{
    public class SecurityKeysViewModel
    {
        public string PrimaryKey { get; set; }

        public string SecondaryKey { get; set; }
    }
}