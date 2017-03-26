// <copyright file="NotificationService.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace PeerInfrastructure.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using Common.Configuration;
    using Common.Helpers;
    using Common.Models;
    using Newtonsoft.Json.Linq;
    using PeerInfrastructure.Repository;
    using PushSharp.Apple;

    public class NotificationService : INotificationService
    {
        private readonly IEpiFibDbContext dbContext;
        private readonly IHashService hashService;
        private readonly ApnsServiceBroker apnsBroker;

        public NotificationService(IConfigurationProvider configProvider, IEpiFibDbContext dbContext, IHashService hashService)
        {
            EFGuard.NotNull(configProvider, nameof(configProvider));
            EFGuard.NotNull(dbContext, nameof(dbContext));
            EFGuard.NotNull(hashService, nameof(hashService));

            this.dbContext = dbContext;
            this.hashService = hashService;

            var p12VirtualPath = configProvider.GetConfigurationSettingValue("push.p12PathToFile");
            var p12PhysicalPath = HttpContext.Current.Server.MapPath(p12VirtualPath);
            var pwd = configProvider.GetConfigurationSettingValue("push.p12Password");

            ApnsConfiguration config;
            try
            {
                config = new ApnsConfiguration(
                    ApnsConfiguration.ApnsServerEnvironment.Sandbox,
                    p12PhysicalPath,
                    pwd);
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Could not create Apn Configuration: {ex}");
                return;
            }

            this.apnsBroker = new ApnsServiceBroker(config);
            this.ConfigureApsnBroker();
            this.apnsBroker.Start();
        }

        public async Task NotifyHashedUserIdsAsync(
            IEnumerable<string> hashedIds, 
            EmergencyInstanceRequest instanceRequest)
        {
            var users = await this.dbContext.Users
                .Where(u => hashedIds.Contains(this.hashService.HashString(u.Id)))
                .ToListAsync();

            ////foreach (EpiFibUser user in users)
            ////{
            ////}
        }

        public async Task Test()
        {
            await Task.Delay(0);
            this.apnsBroker.QueueNotification(new ApnsNotification
            {
                DeviceToken = "FAEE5126C5EFB4389B56609E7172BED0243E31C26EDA7B458E3407ECE7DEA44A",
                Payload = JObject.Parse("{\"aps\":{\"badge\":7,\"alert\" : \"You can be me when you push this clean.\"}}")
            });
        }

        private void ConfigureApsnBroker()
        {
            this.apnsBroker.OnNotificationFailed += (notification, aggregateEx) =>
            {
                aggregateEx.Handle(ex =>
                {
                    // See what kind of exception it was to further diagnose
                    if (ex is ApnsNotificationException)
                    {
                        var notificationException = (ApnsNotificationException)ex;

                        // Deal with the failed notification
                        var apnsNotification = notificationException.Notification;
                        var statusCode = notificationException.ErrorStatusCode;

                        Console.WriteLine($"Apple Notification Failed: ID={apnsNotification.Identifier}, Code={statusCode}");
                    }
                    else
                    {
                        // Inner exception might hold more useful information like an ApnsConnectionException           
                        Console.WriteLine($"Apple Notification Failed for some unknown reason : {ex.InnerException}");
                    }

                    // Mark it as handled
                    return true;
                });
            };

            this.apnsBroker.OnNotificationSucceeded += notification =>
            {
                Console.WriteLine("Apple Notification Sent!");
            };
        }
    }
}
