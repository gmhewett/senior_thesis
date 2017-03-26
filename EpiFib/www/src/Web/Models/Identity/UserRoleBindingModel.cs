// <copyright file="UserRoleBindingModel.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace Web.Models.Identity
{
    public class UserRoleBindingModel
    {
        public string UserId { get; set; }

        public string RoleName { get; set; }
    }
}