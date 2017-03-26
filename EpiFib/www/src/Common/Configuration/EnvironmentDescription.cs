// <copyright file="EnvironmentDescription.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace Common.Configuration
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Xml;
    using System.Xml.XPath;

    public class EnvironmentDescription : IEnvironmentDescription, IDisposable
    {
        private const string ValueAttributeName = "value";
        private const string SettingXpath = "//setting[@name='{0}']";

        private bool isDisposed = false;
        private XmlDocument document = null;
        private XPathNavigator navigator = null;
        private string fileName = null;
        private int updatedValuesCount = 0;
        
        public EnvironmentDescription(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            this.fileName = fileName;
            this.document = new XmlDocument();
            using (XmlReader reader = XmlReader.Create(fileName))
            {
                this.document.Load(reader);
            }

            this.navigator = this.document.CreateNavigator();
        }

        public string GetSetting(string settingName, bool errorOnNull = true)
        {
            if (string.IsNullOrEmpty(settingName))
            {
                throw new ArgumentNullException(nameof(settingName));
            }

            string result = string.Empty;
            XmlNode node = this.GetSettingNode(settingName.Trim());
            if (node != null)
            {
                result = node.Attributes[ValueAttributeName].Value;
            }
            else
            {
                if (errorOnNull)
                {
                    var message = string.Format(CultureInfo.InvariantCulture, "{0} was not found", settingName);
                    throw new ArgumentException(message);
                }
            }

            return result;
        }

        public void Dispose()
        {
            if (!this.isDisposed)
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        private XmlNode GetSettingNode(string settingName)
        {
            string xpath = string.Format(CultureInfo.InvariantCulture, SettingXpath, settingName);
            return this.document.SelectSingleNode(xpath);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.isDisposed = true;
                if (this.updatedValuesCount > 0)
                {
                    this.document.Save(this.fileName);
                    Console.Out.WriteLine("Successfully updated {0} mapping(s) in {1}", this.updatedValuesCount, Path.GetFileName(this.fileName).Split('.')[0]);
                }
            }
        }
    }
}
