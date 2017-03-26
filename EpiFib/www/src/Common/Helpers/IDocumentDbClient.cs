// <copyright file="IDocumentDbClient.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace Common.Helpers
{
    using System.Linq;
    using System.Threading.Tasks;

    public interface IDocumentDbClient<T> where T : new()
    {
        Task<T> GetAsync(string id, string partitionKey);

        Task<IQueryable<T>> QueryAsync();

        Task<T> SaveAsync(T data);

        Task DeleteAsync(string id, string partitionKey);
    }
}
