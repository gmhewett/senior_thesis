// <copyright file="EditDeviceRuleViewModel.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace Web.Models
{
    using System.Collections.Generic;
    using System.Web.Mvc;
    using Resources;

    public class EditDeviceRuleViewModel
    {
        public string RuleId { get; set; }

        public bool EnabledState { get; set; }

        public string DeviceID { get; set; }

        public string DataField { get; set; }

        public string Operator { get; set; }

        public string Threshold { get; set; }

        public string RuleOutput { get; set; }

        public string Etag { get; set; }

        public List<SelectListItem> AvailableDataFields { get; set; }

        public List<SelectListItem> AvailableOperators { get; set; }

        public List<SelectListItem> AvailableRuleOutputs { get; set; }

        public string CheckForErrorMessage()
        {
            double outDouble;
            if (string.IsNullOrWhiteSpace(this.Threshold) || !double.TryParse(this.Threshold, out outDouble))
            {
                return Strings.ThresholdFormatError;
            }

            return null;
        }
    }
}