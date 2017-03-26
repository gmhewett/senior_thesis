// <copyright file="DeviceDisplayHelper.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace Web.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Common.Helpers;
    using Microsoft.Azure.Devices;
    using Resources;
    using Web.Models;
    using StringPair = System.Collections.Generic.KeyValuePair<string, string>;

    public static class DeviceDisplayHelper
    {
        private static readonly HashSet<string> CopyControlDeviceProperties =
            new HashSet<string>(
                new[] 
                {
                    "DEVICEID",
                    "HOSTNAME"
                });

        public static string GetCommandResultClassName(string commandResult)
        {
            FeedbackStatusCode resolvedValue;

            if (Enum.TryParse<FeedbackStatusCode>(
                    commandResult,
                    out resolvedValue))
            {
                switch (resolvedValue)
                {
                    case FeedbackStatusCode.DeliveryCountExceeded:
                        commandResult = "Error";
                        break;

                    case FeedbackStatusCode.Expired:
                        commandResult = "Error";
                        break;

                    case FeedbackStatusCode.Rejected:
                        commandResult = "Error";
                        break;
                }
            }
            else if (string.IsNullOrWhiteSpace(commandResult))
            {
                commandResult = "pending";
            }

            return commandResult;
        }

        public static HashSet<string> BuildAvailableCommandNameSet(DeviceCommandViewModel model)
        {
            EFGuard.NotNull(model, nameof(model));

            IEnumerable<string> commandNames = new string[0];
            if (model.SendCommandModel?.CommandSelectList != null)
            {
                commandNames =
                    commandNames.Concat(
                        model.SendCommandModel.CommandSelectList.Where(
                            t =>
                                !string.IsNullOrWhiteSpace(t?.Value)).Select(u => u.Value));
            }

            return new HashSet<string>(commandNames);
        }

        public static bool GetIsCopyControlPropertyName(string propertyName)
        {
            return !string.IsNullOrEmpty(propertyName) &&
                   CopyControlDeviceProperties.Contains(propertyName.ToUpperInvariant());
        }

        public static StringPair GetLocalizedCommandResultText(
            string commandResult,
            object viewStateErrorMessage)
        {
            FeedbackStatusCode resolvedValue;

            if (string.IsNullOrWhiteSpace(commandResult))
            {
                commandResult = Strings.Pending;
            }

            var errorMessage = viewStateErrorMessage as string;
            if (Enum.TryParse<FeedbackStatusCode>(
                    commandResult,
                    out resolvedValue))
            {
                switch (resolvedValue)
                {
                    case FeedbackStatusCode.DeliveryCountExceeded:
                        errorMessage = Strings.CommandDeliveryCountExceeded;
                        commandResult = "Error";
                        break;

                    case FeedbackStatusCode.Expired:
                        errorMessage = Strings.CommandExpired;
                        commandResult = "Error";
                        break;

                    case FeedbackStatusCode.Rejected:
                        errorMessage = Strings.CommandRejected;
                        commandResult = "Error";
                        break;

                    case FeedbackStatusCode.Success:
                        errorMessage = string.Empty;
                        commandResult = Strings.CommandSuccess;
                        break;
                }
            }

            return new StringPair(commandResult, errorMessage);
        }

        public static string GetDevicePropertyFieldLocalName(string fieldName)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
            {
                return fieldName;
            }

            string resourceName = string.Format(CultureInfo.InvariantCulture, "DeviceProperty_{0}", fieldName);
            string resourceValue = Strings.ResourceManager.GetString(resourceName);

            return resourceValue ?? fieldName;
        }
    }
}