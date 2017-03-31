// <copyright file="ContainerAlarmCommand.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace Common.Models
{
    public class ContainerAlarmCommand
    {
        public string EmergencyInstanceId { get; set; }

        public string ContainerId { get; set; }

        public int Value { get; set; }
    }
}