// <copyright file="Error.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace IoTInfrastructure.Models
{
    using System;
    using IoTInfrastructure.Properties;

    [Serializable]
    public class Error
    {
        public Error()
        {
            this.Type = ErrorType.Exception;
            this.Message = Resources.UnexpectedErrorOccurred;
        }

        public Error(string validationError)
        {
            this.Type = ErrorType.Validation;
            this.Message = validationError;
        }

        public enum ErrorType
        {
            Exception = 0,
            Validation = 1
        }

        public ErrorType Type { get; set; }

        public string Message { get; set; }
    }
}
