// <copyright file="ParameterViewModel.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace Web.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using IoTInfrastructure.Services;
    using Resources;

    public class ParameterViewModel
    {
        public ParameterViewModel()
        {
            this.ErrorMessages = new List<string>();
        }

        public string Name { get; set; }

        public string Type { get; set; }

        public string Value { get; set; }

        public List<string> ErrorMessages { get; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResult = new List<ValidationResult>();
            var isTypeValid = CommandParameterTypeService.Instance.IsValid(this.Type, this.Value);
            if (!isTypeValid)
            {
                string errorMessage = this.GetCommandErrorMessage();
                validationResult.Add(new ValidationResult(errorMessage));
                this.ErrorMessages.Add(errorMessage);
            }

            return validationResult;
        }

        private string GetCommandErrorMessage()
        {
            string errorMessage = Strings.ResourceManager.GetString(string.Format(
                CultureInfo.InvariantCulture,
                "{0}CommandErrorMessage",
                this.Type.ToLowerInvariant()));

            if (string.IsNullOrWhiteSpace(errorMessage))
            {
                errorMessage = string.Format(
                    CultureInfo.CurrentCulture,
                    Strings.UnknownCommandParameterType,
                    this.Type);
            }

            return errorMessage;
        }
    }
}