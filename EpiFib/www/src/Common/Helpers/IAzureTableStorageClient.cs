// <copyright file="IAzureTableStorageClient.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace Common.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Common.Models;
    using Microsoft.WindowsAzure.Storage.Table;

    public interface IAzureTableStorageClient
    {
        TableResult Execute(TableOperation tableOperation);

        Task<TableResult> ExecuteAsync(TableOperation operation);

        IEnumerable<T> ExecuteQuery<T>(TableQuery<T> tableQuery) where T : TableEntity, new();

        Task<IEnumerable<T>> ExecuteQueryAsync<T>(TableQuery<T> tableQuery) where T : TableEntity, new();

        Task<TableStorageResponse<TResult>> DoTableInsertOrReplaceAsync<TResult, TInput>(
            TInput incomingEntity,
            Func<TInput, TResult> tableEntityToModelConverter) where TInput : TableEntity;

        Task<TableStorageResponse<TResult>> DoDeleteAsync<TResult, TInput>(
            TInput incomingEntity,
            Func<TInput, TResult> tableEntityToModelConverter) where TInput : TableEntity;
    }
}
