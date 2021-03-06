﻿// <copyright file="SimulatorModule.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace Simulator.WebJob
{
    using Autofac;
    using Common.Configuration;
    using Common.Helpers;
    using Common.Repository;
    using IoTInfrastructure.Repository;
    using IoTInfrastructure.Services;
    using Simulator.WebJob.DataInitialization;

    public class SimulatorModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ConfigurationProvider>().As<IConfigurationProvider>().SingleInstance();
            builder.RegisterType<DeviceService>().As<IDeviceService>();
            builder.RegisterType<DeviceRulesService>().As<IDeviceRulesService>();
            builder.RegisterType<ActionService>().As<IActionService>();
            builder.RegisterType<ActionMappingService>().As<IActionMappingService>();
            builder.RegisterType<IoTHubDeviceManager>().As<IIoTHubDeviceManager>();
            builder.RegisterType<SecurityKeyGenerator>().As<ISecurityKeyGenerator>();
            builder.RegisterType<DataInitializer>().As<IDataInitializer>();
            builder.RegisterType<IoTHubRepository>().As<IIoTHubRepository>();
            builder.RegisterType<DeviceRegistryRepository>().As<IDeviceRegistryCrudRepository>();
            builder.RegisterType<DeviceRegistryRepository>().As<IDeviceRegistryListRepository>();
            builder.RegisterType<DeviceRulesRepository>().As<IDeviceRulesRepository>();
            builder.RegisterType<ActionMappingRepository>().As<IActionMappingRepository>();
            builder.RegisterType<ActionRepository>().As<IActionRepository>();
            builder.RegisterType<VirtualDeviceTableStorage>().As<IVirtualDeviceStorage>();
            builder.RegisterType<AzureTableStorageClientFactory>().As<IAzureTableStorageClientFactory>();
            builder.RegisterType<BlobStorageClientFactory>().As<IBlobStorageClientFactory>();
            builder.RegisterGeneric(typeof(DocumentDbClientBase<>)).As(typeof(IDocumentDbClientBase<>));
            builder.RegisterGeneric(typeof(DeviceDocumentDbClient<>)).As(typeof(IDeviceDocumentDbClient<>));
        }
    }
}