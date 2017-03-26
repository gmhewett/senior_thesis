// <copyright file="AzureTableStorageClient.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace Common.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using Common.Models;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    public class AzureTableStorageClient : IAzureTableStorageClient
    {
        private readonly CloudTableClient tableClient;
        private readonly string tableName;
        private CloudTable cloudTable;

        public AzureTableStorageClient(string storageConnectionString, string tableName)
        {
            var storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            this.tableClient = storageAccount.CreateCloudTableClient();
            this.tableName = tableName;
        }
        
        public TableResult Execute(TableOperation tableOperation)
        {
            CloudTable table = this.GetCloudTable();
            return table.Execute(tableOperation);
        }

        public async Task<TableResult> ExecuteAsync(TableOperation operation)
        {
            CloudTable table = await this.GetCloudTableAsync();
            return await table.ExecuteAsync(operation);
        }

        public IEnumerable<T> ExecuteQuery<T>(TableQuery<T> tableQuery) where T : TableEntity, new()
        {
            CloudTable table = this.GetCloudTable();
            return table.ExecuteQuery(tableQuery);
        }

        public async Task<IEnumerable<T>> ExecuteQueryAsync<T>(TableQuery<T> tableQuery) where T : TableEntity, new()
        {
            CloudTable table = await this.GetCloudTableAsync();
            return table.ExecuteQuery(tableQuery);
        }

        public async Task<TableStorageResponse<TResult>> DoTableInsertOrReplaceAsync<TResult, TInput>(TInput incomingEntity, Func<TInput, TResult> tableEntityToModelConverter) where TInput : TableEntity
        {
            CloudTable table = await this.GetCloudTableAsync();

            var retrieveOperation =
                TableOperation.Retrieve<TInput>(incomingEntity.PartitionKey, incomingEntity.RowKey);
            var retrievedEntity = await table.ExecuteAsync(retrieveOperation);

            var operation = retrievedEntity.Result != null
                ? TableOperation.Replace(incomingEntity)
                : TableOperation.Insert(incomingEntity);

            return await PerformTableOperation(operation, incomingEntity, tableEntityToModelConverter);
        }

        public async Task<TableStorageResponse<TResult>> DoDeleteAsync<TResult, TInput>(TInput incomingEntity, Func<TInput, TResult> tableEntityToModelConverter) where TInput : TableEntity
        {
            var operation = TableOperation.Delete(incomingEntity);
            return await PerformTableOperation(operation, incomingEntity, tableEntityToModelConverter);
        }

        private async Task<CloudTable> GetCloudTableAsync()
        {
            if (this.cloudTable != null)
            {
                return this.cloudTable;
            }

            this.cloudTable = this.tableClient.GetTableReference(this.tableName);
            await this.cloudTable.CreateIfNotExistsAsync();
            return this.cloudTable;
        }

        private CloudTable GetCloudTable()
        {
            if (this.cloudTable != null)
            {
                return this.cloudTable;
            }

            this.cloudTable = this.tableClient.GetTableReference(this.tableName);
            this.cloudTable.CreateIfNotExists();
            return this.cloudTable;
        }

        private async Task<TableStorageResponse<TResult>> PerformTableOperation<TResult, TInput>(
            TableOperation operation, 
            TInput incomingEntity, 
            Func<TInput, TResult> tableEntityToModelConverter)
            where TInput : TableEntity
        {
            await this.GetCloudTableAsync();

            var result = new TableStorageResponse<TResult>();
            try
            {
                await this.cloudTable.ExecuteAsync(operation);

                TResult nullModel = tableEntityToModelConverter(null);
                result.Entity = nullModel;
                result.Status = TableStorageResponseStatus.Successful;
            }
            catch (Exception ex)
            {
                TableOperation retrieveOperation = TableOperation.Retrieve<TInput>(
                    incomingEntity.PartitionKey,
                    incomingEntity.RowKey);
                TableResult retrievedEntity = this.cloudTable.Execute(retrieveOperation);

                if (retrievedEntity != null)
                {
                    TResult retrievedModel = tableEntityToModelConverter((TInput)retrievedEntity.Result);
                    result.Entity = retrievedModel;
                }
                else
                {
                    result.Entity = tableEntityToModelConverter(incomingEntity);
                }

                result.Status = ex.GetType() == typeof(StorageException)
                                &&
                                (((StorageException)ex).RequestInformation.HttpStatusCode ==
                                 (int)HttpStatusCode.PreconditionFailed
                                 ||
                                 ((StorageException)ex).RequestInformation.HttpStatusCode ==
                                 (int)HttpStatusCode.Conflict)
                    ? TableStorageResponseStatus.ConflictError
                    : TableStorageResponseStatus.UnknownError;
            }

            return result;
        }
    }
}
