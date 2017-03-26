// <copyright file="DashboardViewModel.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace Web.Models
{
    using System.Collections.Generic;
    using StringPair = System.Collections.Generic.KeyValuePair<string, string>;

    public class DashboardViewModel
    {
        public DashboardViewModel()
        {
            this.DeviceIdsForDropdown = new List<StringPair>();
        }

        public List<StringPair> DeviceIdsForDropdown { get; private set; }

        public string MapApiQueryKey { get; set; }

        public string Username { get; set; }

        public string AppInsightsKey { get; set; }

        public IEnumerable<LanguageModel> AvailableLanguages { get; set; }

        public string CurrentLanguageNameIso { get; set; }

        public string CurrentLanguageName { get; set; }

        public string CurrentLanguageTextDirection { get; set; }
    }
}