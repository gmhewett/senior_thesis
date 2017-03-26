// <copyright file="IUserLocationDocumentDbClient.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace PeerInfrastructure.Helpers
{
    using Common.Helpers;

    public interface IUserLocationDocumentDbClient<T> : IDocumentDbClient<T> where T : new()
    {
    }
}
