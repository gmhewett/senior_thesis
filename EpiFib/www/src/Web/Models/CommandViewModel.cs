// <copyright file="CommandViewModel.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace Web.Models
{
    using System.Collections.Generic;

    public class CommandViewModel
    {
        private List<ParameterViewModel> parameters;

        public List<ParameterViewModel> Parameters
        {
            get
            {
                if (this.parameters?.Count == 1 && this.parameters[0] == null)
                {
                    this.parameters = new List<ParameterViewModel>();
                }

                return this.parameters;
            }

            set
            {
                this.parameters = value;
            }
        }

        public string Name { get; set; }

        public string DeviceId { get; set; }
    }
}