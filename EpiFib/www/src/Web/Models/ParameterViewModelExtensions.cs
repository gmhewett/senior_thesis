// <copyright file="ParameterViewModelExtensions.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace Web.Models
{
    using System.Collections.Generic;
    using System.Linq;
    using Common.Models;

    public static class ParameterViewModelExtensions
    {
        public static IEnumerable<ParameterViewModel> ToParametersModel(this List<Parameter> parameters)
        {
            if (parameters == null || parameters.Count == 0)
            {
                return new List<ParameterViewModel>();
            }

            return parameters.Select(parameter => new ParameterViewModel
            {
                Name = parameter.Name,
                Type = parameter.Type
            });
        }
    }
}