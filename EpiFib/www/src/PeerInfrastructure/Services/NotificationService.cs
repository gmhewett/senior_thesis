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
    using PeerInfrastructure.Models;
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

        public async Task<IEnumerable<string>> NotifyHashedUserIdsAsync(
            IEnumerable<string> hashedIds, 
            EmergencyInstanceRequest instanceRequest)
        {
            var users = await this.dbContext.Users
                .Where(u => u.DeviceToken != null && u.DeviceToken != string.Empty)
                .Select(u => new { u.Id, u.DeviceToken })
                .ToListAsync();

            foreach (var user in users)
            {
                if (hashedIds.Contains(this.hashService.HashString(user.Id)) && !string.IsNullOrWhiteSpace(user.DeviceToken))
                {
                    var payload = new EmergencyInstanceApnsNotfication
                    {
                        aps = new ApnsNotificationPayload
                        {
                            alert = "Someone needs your help right away!",
                            badge = "1"
                        },
                        emergencyInstance = instanceRequest
                    };

                    var notification = new ApnsNotification
                    {
                        DeviceToken = user.DeviceToken,
                        Payload = JObject.FromObject(payload)
                    };
                    this.apnsBroker.QueueNotification(notification);
                }
            }

            return users.Select(u => u.Id).ToList();
        }

        public async Task Test()
        {
            await Task.Delay(0);
            this.apnsBroker.QueueNotification(new ApnsNotification
            {
                DeviceToken = "6301FAF46B15FFD7D679601CC25F85171A23A11FAA83FA8E994F272EF68CBF7B",
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
