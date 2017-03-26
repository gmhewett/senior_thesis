// <copyright file="FilterType.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace IoTInfrastructure.Models
{
    public enum FilterType
    {
        Status,
        ExactMatchCaseSensitive,
        ExactMatchCaseInsensitive,
        StartsWithCaseSensitive,
        StartsWithCaseInsensitive,
        ContainsCaseSensitive,
        ContainsCaseInsensitive
    }
}
