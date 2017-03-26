// <copyright file="LanguageModel.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace Web.Models
{
    public sealed class LanguageModel
    {
        public string Name { get; set; }

        public string CultureName { get; set; }

        public bool IsCurrent { get; set; }
    }
}