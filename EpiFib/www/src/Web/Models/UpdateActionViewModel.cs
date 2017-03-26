// <copyright file="UpdateActionViewModel.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace Web.Models
{
    using System.Collections.Generic;
    using System.Web.Mvc;

    public class UpdateActionViewModel
    {
        public string RuleOutput { get; set; }

        public string ActionId { get; set; }

        public List<SelectListItem> ActionSelectList { get; set; }
    }
}