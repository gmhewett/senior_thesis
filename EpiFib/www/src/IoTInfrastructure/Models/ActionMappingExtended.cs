// <copyright file="ActionMappingExtended.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace IoTInfrastructure.Models
{
    public class ActionMappingExtended : ActionMapping
    {
        public int NumberOfDevices { get; set; }
    }
}
