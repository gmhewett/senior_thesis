// <copyright file="WebModule.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace Web
{
    using Autofac;
    using Common.Configuration;
    using Common.Helpers;
    using Common.Repository;
    using IoTInfrastructure.Repository;
    using IoTInfrastructure.Services;
    using PeerInfrastructure.Helpers;
    using PeerInfrastructure.Repository;
    using PeerInfrastructure.Services;

    public sealed class WebModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ConfigurationProvider>().As<IConfigurationProvider>();
            builder.RegisterType<DeviceService>().As<IDeviceService>();
            builder.RegisterType<DeviceRulesService>().As<IDeviceRulesService>();
            builder.RegisterType<ActionMappingService>().As<IActionMappingService>();
            builder.RegisterType<ActionService>().As<IActionService>();
            builder.RegisterType<DeviceTypeService>().As<IDeviceTypeService>();
            builder.RegisterType<CommandParameterTypeService>().As<ICommandParameterTypeService>();
            builder.RegisterType<DeviceTelemetryService>().As<IDeviceTelemetryService>();
            builder.RegisterType<AlertsService>().As<IAlertsService>();
            builder.RegisterType<EmergencyInstanceService>().As<IEmergencyInstanceService>();
            builder.RegisterType<UserLocationService>().As<IUserLocationService>();
            builder.RegisterType<HashService>().As<IHashService>();
            builder.RegisterType<NotificationService>().As<INotificationService>();
            builder.RegisterType<SecurityKeyGenerator>().As<ISecurityKeyGenerator>();
            builder.RegisterType<IoTHubDeviceManager>().As<IIoTHubDeviceManager>();
            builder.RegisterType<IoTHubRepository>().As<IIoTHubRepository>();
            builder.RegisterType<EpiFibDbContext>().As<IEpiFibDbContext>().InstancePerRequest();
            builder.RegisterType<DeviceRegistryRepository>().As<IDeviceRegistryCrudRepository>();
            builder.RegisterType<DeviceRegistryRepository>().As<IDeviceRegistryListRepository>();
            builder.RegisterType<DeviceRulesRepository>().As<IDeviceRulesRepository>();
            builder.RegisterType<ActionMappingRepository>().As<IActionMappingRepository>();
            builder.RegisterType<ActionRepository>().As<IActionRepository>();
            builder.RegisterType<DeviceTypeRepository>().As<IDeviceTypeRepository>();
            builder.RegisterType<DeviceTelemetryRepository>().As<IDeviceTelemetryRepository>();
            builder.RegisterType<AlertsRepository>().As<IAlertsRepository>();
            builder.RegisterType<EmergencyInstanceRepository>().As<IEmergencyInstanceRepository>();
            builder.RegisterType<UserLocationRepository>().As<IUserLocationRepository>();
            builder.RegisterType<VirtualDeviceTableStorage>().As<IVirtualDeviceStorage>();
            builder.RegisterType<AzureTableStorageClientFactory>().As<IAzureTableStorageClientFactory>();
            builder.RegisterType<BlobStorageClientFactory>().As<IBlobStorageClientFactory>();
            builder.RegisterGeneric(typeof(DocumentDbClientBase<>)).As(typeof(IDocumentDbClientBase<>));
            builder.RegisterGeneric(typeof(DeviceDocumentDbClient<>)).As(typeof(IDeviceDocumentDbClient<>));
            builder.RegisterGeneric(typeof(UserLocationDocumentDbClient<>)).As(typeof(IUserLocationDocumentDbClient<>));
            builder.RegisterGeneric(typeof(EmergencyInstanceDocumentDbClient<>)).As(typeof(IEmergencyInstanceDocumentDbClient<>));
        }
    }
}