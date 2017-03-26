// <copyright file="AzureRetryHelper.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace Common.Helpers
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Common.Exceptions;

    public static class AzureRetryHelper
    {
        private const int RetryCount = 2;

        public static async Task OperationWithBasicRetryAsync(Func<Task> asyncOperation)
        {
            await OperationWithBasicRetryAsync<object>(async () =>
            {
                await asyncOperation();
                return null;
            });
        }

        public static async Task<T> OperationWithBasicRetryAsync<T>(Func<Task<T>> asyncOperation)
        {
            int currentRetry = 0;

            while (true)
            {
                try
                {
                    return await asyncOperation();
                }
                catch (Exception ex)
                {
                    currentRetry++;

                    if (currentRetry > RetryCount || !IsTransient(ex))
                    {
                        // If this is not a transient error or we should not retry re-throw the exception. 
                        throw;
                    }
                }

                // Wait to retry the operation.  
                await Task.Delay(100 * currentRetry);
            }
        }

        private static bool IsTransient(Exception originalException)
        {
            // If the exception is a IotHubException its IsTransient property can be inspected
            var iotHubException = originalException as IotHubException;
            if (iotHubException != null)
            {
                return iotHubException.IsTransient;
            }

            // If the exception is an HTTP request exception then assume it is transient
            var httpException = originalException as HttpRequestException;
            if (httpException != null)
            {
                return true;
            }

            WebException webException = originalException as WebException;
            if (webException != null)
            {
                // If the web exception contains one of the following status values  it may be transient.
                return new[]
                {
                    WebExceptionStatus.ConnectionClosed,
                    WebExceptionStatus.Timeout,
                    WebExceptionStatus.RequestCanceled
                }.Contains(webException.Status);
            }

            return false;
        }
    }
}
