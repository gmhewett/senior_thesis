// <copyright file="CommandParameterTypeService.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace IoTInfrastructure.Services
{
    using System;
    using System.Globalization;
    using IoTInfrastructure.Models;

    public class CommandParameterTypeService : ICommandParameterTypeService
    {
        private static readonly Lazy<CommandParameterTypeService> CommandParameterTypeServiceInstance = 
            new Lazy<CommandParameterTypeService>(() => new CommandParameterTypeService());

        public static CommandParameterTypeService Instance => CommandParameterTypeServiceInstance.Value;

        public bool IsValid(string typeName, object value)
        {
            return this.IsTypeValid(typeName, value);
        }

        public object Get(string typeName, object value)
        {
            var lowerCaseTypeName = typeName.ToLowerInvariant();

            Type type;

            CommandTypes.Types.TryGetValue(lowerCaseTypeName, out type);

            if (value == null && this.CanTypeBeNull(lowerCaseTypeName))
            {
                return null;
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            string parameterValue = value.ToString();

            return this.CommandValueFactory(lowerCaseTypeName, parameterValue, type);
        }

        private static object ParseBase64(string base64String)
        {
            bool isBase64;
            if (string.IsNullOrWhiteSpace(base64String) ||
                base64String.Length % 4 != 0 ||
                base64String.Contains(" ") ||
                base64String.Contains("\t") ||
                base64String.Contains("\r") ||
                base64String.Contains("\n"))
            {
                // ReSharper disable once RedundantAssignment
                isBase64 = false;
            }

            // now do the real test
            try
            {
                // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                if (base64String != null)
                {
                    Convert.FromBase64String(base64String);
                }

                isBase64 = true;
            }
            catch (FormatException)
            {
                isBase64 = false;
            }

            return isBase64 ? base64String : null;
        }

        private static object ReturnDateTimeOffset(object value)
        {
            DateTime datetime;

            var dateString = value.ToString();
            var isValid = DateTime.TryParse(dateString, out datetime);

            if (!isValid)
            {
                return null;
            }

            return datetime.ToUniversalTime();
        }

        private static object ParseDecimal(string value)
        {
            decimal parsedValue;
            var isValid = decimal.TryParse(value, out parsedValue);
            return !isValid ? null : parsedValue.ToString(CultureInfo.InvariantCulture);
        }

        private static object ParseInt64(string value)
        {
            long parsedValue;
            var isValid = long.TryParse(value, out parsedValue);
            return !isValid ? null : parsedValue.ToString(CultureInfo.InvariantCulture);
        }

        private static object ParseGuid(string value)
        {
            Guid guid;
            var isValid = Guid.TryParse(value, out guid);

            if (!isValid)
            {
                return null;
            }

            return guid;
        }

        private static object ParseDouble(string value)
        {
            double doubleValue;
            var isValid = double.TryParse(value, out doubleValue);

            if (!isValid)
            {
                return null;
            }

            return doubleValue;
        }

        private static object ParseDate(string value)
        {
            DateTime datetime;
            bool isValid = DateTime.TryParse(value, out datetime);

            return !isValid ? null : datetime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        private bool IsTypeValid(string typeName, object value)
        {
            try
            {
                if (value == null && this.CanTypeBeNull(typeName.ToLowerInvariant()))
                {
                    return true;
                }

                var parsedValue = this.Get(typeName, value);

                return parsedValue != null;
            }
            catch
            {
                return false;
            }
        }

        private bool CanTypeBeNull(string typeName)
        {
            return typeName == "string" || typeName == "binary";
        }

        private object CommandValueFactory(string typeName, string value, Type type)
        {
            switch (typeName)
            {
                case "datetimeoffset":
                    return ReturnDateTimeOffset(value);

                case "date":
                    return ParseDate(value);

                case "double":
                    return ParseDouble(value);

                case "guid":
                    return ParseGuid(value);

                case "int64":
                    return ParseInt64(value);

                case "decimal":
                    return ParseDecimal(value);

                case "binary":
                    return ParseBase64(value);

                default:
                    return Convert.ChangeType(
                        value,
                        type,
                        CultureInfo.CurrentCulture);
            }
        }
    }
}
