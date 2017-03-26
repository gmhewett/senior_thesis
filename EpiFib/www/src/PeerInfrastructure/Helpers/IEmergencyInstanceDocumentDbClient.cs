// <copyright file="IEmergencyInstanceDocumentDbClient.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace PeerInfrastructure.Helpers
{
    using Common.Helpers;

    public interface IEmergencyInstanceDocumentDbClient<T> : IDocumentDbClient<T> where T : new()
    {
    }
}
