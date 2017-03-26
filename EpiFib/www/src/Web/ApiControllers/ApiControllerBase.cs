// <copyright file="ApiControllerBase.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace Web.ApiControllers
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Common.Helpers;
    using IoTInfrastructure.Exceptions;
    using IoTInfrastructure.Models;
    using Resources;

    public abstract class ApiControllerBase : ApiController
    {
        protected async Task<HttpResponseMessage> GetServiceResponseAsync(Func<Task> getData)
        {
            EFGuard.NotNull(getData, nameof(getData));

            return await this.GetServiceResponseAsync<object>(async () =>
            {
                await getData();
                return null;
            });
        }

        protected async Task<HttpResponseMessage> GetServiceResponseAsync<T>(Func<Task<T>> getData)
        {
            EFGuard.NotNull(getData, nameof(getData));
            
            return await this.GetServiceResponseAsync(getData, true);
        }

        protected async Task<HttpResponseMessage> GetServiceResponseAsync<T>(Func<Task<T>> getData, bool useServiceResponse)
        {
            EFGuard.NotNull(getData, nameof(getData));

            ServiceResponse<T> response = new ServiceResponse<T>();
            try
            {
                response.Data = await getData();
            }
            catch (ValidationException ex)
            {
                if (ex.Errors == null || ex.Errors.Count == 0)
                {
                    response.Error.Add(new Error("Unknown validation error"));
                }
                else
                {
                    foreach (string error in ex.Errors)
                    {
                        response.Error.Add(new Error(error));
                    }
                }
            }
            catch (DeviceAdministrationExceptionBase ex)
            {
                response.Error.Add(new Error(ex.Message));
            }
            catch (HttpResponseException)
            {
                throw;
            }
            catch (Exception ex)
            {
                response.Error.Add(new Error());
                Debug.Write(FormatExceptionMessage(ex), " GetServiceResponseAsync Exception");
            }

            // if there's an error or we've been asked to use a service response, then return a service response
            if (response.Error.Count > 0 || useServiceResponse)
            {
                return Request.CreateResponse(
                        response.Error != null && response.Error.Any() ? HttpStatusCode.BadRequest : HttpStatusCode.OK,
                        response);
            }

            // otherwise there's no error and we need to return the data at the root of the response
            return Request.CreateResponse(HttpStatusCode.OK, response.Data);
        }

        protected HttpResponseMessage GetNullRequestErrorResponse<T>()
        {
            ServiceResponse<T> response = new ServiceResponse<T>();
            response.Error.Add(new Error(Strings.RequestNullError));

            return Request.CreateResponse(HttpStatusCode.BadRequest, response);
        }

        protected HttpResponseMessage GetFormatErrorResponse<T>(string parameterName, string type)
        {
            ServiceResponse<T> response = new ServiceResponse<T>();

            string errorMessage =
                string.Format(
                    CultureInfo.CurrentCulture,
                    Strings.RequestFormatError,
                    parameterName, 
                    type);

            response.Error.Add(new Error(errorMessage));

            return Request.CreateResponse(HttpStatusCode.BadRequest, response);
        }

        protected void TerminateProcessingWithMessage(HttpStatusCode statusCode, string message)
        {
            HttpResponseMessage responseMessage = new HttpResponseMessage()
            {
                StatusCode = statusCode,
                ReasonPhrase = message
            };

            throw new HttpResponseException(responseMessage);
        }

        protected void ValidateArgumentNotNullOrWhitespace(string argumentName, string value)
        {
            Debug.Assert(
                !string.IsNullOrWhiteSpace(argumentName),
                "argumentName is a null reference, empty string, or contains only whitespace.");

            if (string.IsNullOrWhiteSpace(value))
            {
                // Error strings are not localized.
                string errorText =
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "{0} is null, empty, or just whitespace.",
                        argumentName);

                this.TerminateProcessingWithMessage(HttpStatusCode.BadRequest, errorText);
            }
        }

        protected void ValidateArgumentNotNull(string argumentName, object value)
        {
            Debug.Assert(
                !string.IsNullOrWhiteSpace(argumentName),
                "argumentName is a null reference, empty string, or contains only whitespace.");

            if (value == null)
            {
                // Error strings are not localized.
                string errorText = string.Format(CultureInfo.InvariantCulture, "{0} is a null reference.", argumentName);

                this.TerminateProcessingWithMessage(HttpStatusCode.BadRequest, errorText);
            }
        }

        protected void ValidatePositiveValue(string argumentName, int value)
        {
            Debug.Assert(
                !string.IsNullOrWhiteSpace(argumentName),
                "argumentName is a null reference, empty string, or contains only whitespace.");

            if (value <= 0)
            {
                // Error strings are not localized.
                string errorText =
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "{0} is not a positive integer.",
                        argumentName);

                this.TerminateProcessingWithMessage(HttpStatusCode.BadRequest, errorText);
            }
        }

        private static string FormatExceptionMessage(Exception ex)
        {
            EFGuard.NotNull(ex, nameof(ex));

            // Error strings are not localized
            return string.Format(
                CultureInfo.CurrentCulture,
                "{0}{0}*** EXCEPTION ***{0}{0}{1}{0}{0}",
                Console.Out.NewLine,
                ex);
        }
    }
}