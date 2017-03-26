// <copyright file="ActionPropertiesViewModel.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace Web.Models
{
    using Web.Security;

    public class ActionPropertiesViewModel
    {
        public string RuleOutput { get; set; }

        public string ActionId { get; set; }

        public bool HasAssignActionPerm => PermsChecker.HasPermission(Permission.AssignAction);

        public UpdateActionViewModel UpdateActionModel { get; set; }
    }
}