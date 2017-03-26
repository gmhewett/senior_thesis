// <copyright file="IEnvironmentDescription.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace Common.Configuration
{
    public interface IEnvironmentDescription
    {
        string GetSetting(string settingName, bool errorOnNull = true);
    }
}
