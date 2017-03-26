// <copyright file="IConfigurationProvider.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace Common.Configuration
{
    public interface IConfigurationProvider
    {
        string GetConfigurationSettingValue(string configurationSettingName);

        string GetConfigurationSettingValueOrDefault(string configurationSettingName, string defaultValue);
    }
}
