// <copyright file="ICommandParameterTypeService.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace IoTInfrastructure.Services
{
    public interface ICommandParameterTypeService
    {
        bool IsValid(string typeName, object value);

        object Get(string typeName, object value);
    }
}
