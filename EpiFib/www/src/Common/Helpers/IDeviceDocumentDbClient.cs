// <copyright file="IDeviceDocumentDbClient.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace Common.Helpers
{
    public interface IDeviceDocumentDbClient<T> : IDocumentDbClient<T> where T : new()
    {
    }
}
