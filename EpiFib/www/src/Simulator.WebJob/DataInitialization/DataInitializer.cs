// <copyright file="DataInitializer.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace Simulator.WebJob.DataInitialization
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Common.Helpers;
    using IoTInfrastructure.Models;
    using IoTInfrastructure.Services;

    public class DataInitializer : IDataInitializer
    {
        private readonly IActionMappingService actionMappingService;
        private readonly IDeviceService deviceService;
        private readonly IDeviceRulesService deviceRulesService;

        public DataInitializer(
            IActionMappingService actionMappingService,
            IDeviceService deviceService,
            IDeviceRulesService deviceRulesService)
        {
            EFGuard.NotNull(actionMappingService, nameof(actionMappingService));
            EFGuard.NotNull(deviceService, nameof(deviceService));
            EFGuard.NotNull(deviceRulesService, nameof(deviceRulesService));
            
            this.actionMappingService = actionMappingService;
            this.deviceService = deviceService;
            this.deviceRulesService = deviceRulesService;
        }

        public async void CreateInitialDataIfNeeded()
        {
            try
            {
                bool initializationNeeded = false;

                // check if action mappings are there
                Task.Run(async () => initializationNeeded = await this.actionMappingService.IsInitializationNeededAsync()).Wait();

                if (!initializationNeeded)
                {
                    Trace.TraceInformation("No initial data needed.");
                    return;
                }

                Trace.TraceInformation("Beginning initial data creation...");

                List<string> bootstrappedDevices = null;

                // 1) create default devices
                Task.Run(async () => bootstrappedDevices = await this.deviceService.BootstrapDefaultDevices()).Wait();

                // 2) create default rules
                Task.Run(() => this.deviceRulesService.BootstrapDefaultRulesAsync(bootstrappedDevices)).Wait();

                // 3) create action mappings (do this last to ensure that we'll try to 
                //    recreate if any of the above throws)
                await this.actionMappingService.InitializeDataIfNecessaryAsync();

                Trace.TraceInformation("Initial data creation completed.");
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failed to create initial default data: {0}", ex.ToString());
            }
        }
    }
}
