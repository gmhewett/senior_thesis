// <copyright file="IDocumentDbClientBase.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace Common.Helpers
{
    using System.Linq;
    using System.Threading.Tasks;

    public interface IDocumentDbClientBase<T> where T : new()
    {
        Task<T> GetAsync(string id, string partitionKey, string databaseId, string collectionName);

        Task<IQueryable<T>> QueryAsync(string databaseId, string collectionName);

        Task<T> SaveAsync(T data, string databaseId, string collectionName);
        
        Task DeleteAsync(string id, string partitionKey, string databaseId, string collectionName);
    }
}
