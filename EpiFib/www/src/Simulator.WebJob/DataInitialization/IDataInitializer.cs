// <copyright file="IDataInitializer.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace Simulator.WebJob.DataInitialization
{
    public interface IDataInitializer
    {
        void CreateInitialDataIfNeeded();
    }
}
