// <copyright file="ConfigurationProvider.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace Common.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using Microsoft.Azure;

    public class ConfigurationProvider : IConfigurationProvider, IDisposable
    {
        private const string ConfigToken = "config:";
        private const string SrcPath = "EpiFib\\www\\src";
        private const string PathToDevConfigFileName = "\\Common\\EpiFibDev.config";

        private readonly Dictionary<string, string> configuration = new Dictionary<string, string>();
        private EnvironmentDescription environment = null;
        private bool isDisposed = false;

        ~ConfigurationProvider()
        {
            this.Dispose(false);
        }

        public string GetConfigurationSettingValue(string configurationSettingName)
        {
            return this.GetConfigurationSettingValueOrDefault(configurationSettingName, string.Empty);
        }

        public string GetConfigurationSettingValueOrDefault(string configurationSettingName, string defaultValue)
        {
            if (!this.configuration.ContainsKey(configurationSettingName))
            {
                string configValue = CloudConfigurationManager.GetSetting(configurationSettingName);
                bool isEmulated = Environment.CommandLine.Contains("iisexpress.exe") ||
                    Environment.CommandLine.Contains("w3wp.exe") ||
                    Environment.CommandLine.Contains("WebJob.vshost.exe");

                if (isEmulated && (configValue != null && configValue.StartsWith(ConfigToken, StringComparison.OrdinalIgnoreCase)))
                {
                    if (this.environment == null)
                    {
                        this.LoadEnvironmentConfig();
                    }

                    configValue = this.environment.GetSetting(
                        configValue.Substring(configValue.IndexOf(ConfigToken, StringComparison.Ordinal) + ConfigToken.Length));
                }

                try
                {
                    this.configuration.Add(configurationSettingName, configValue);
                }
                catch (ArgumentException)
                {
                    // key already added, continue
                }
            }

            return this.configuration[configurationSettingName];
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                if (this.environment != null)
                {
                    this.environment.Dispose();
                }
            }

            this.isDisposed = true;
        }

        private void LoadEnvironmentConfig()
        {
            string executingPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            if (string.IsNullOrWhiteSpace(executingPath))
            {
                throw new ArgumentNullException(nameof(executingPath), "Unable to determine executing path.");
            }

            int solutionLocation = executingPath.IndexOf(SrcPath, StringComparison.OrdinalIgnoreCase);
            if (solutionLocation >= 0)
            {
                string fileName = executingPath.Substring(0, solutionLocation) + SrcPath + PathToDevConfigFileName;
                if (File.Exists(fileName))
                {
                    this.environment = new EnvironmentDescription(fileName);
                    return;
                }
            }

            throw new ArgumentException("Unable to locate local.config.user file.  Make sure you have run 'build.cmd local'.");
        }
    }
}
