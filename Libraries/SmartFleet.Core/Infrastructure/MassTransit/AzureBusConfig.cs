using System;
using System.Configuration;
using Autofac;
using MassTransit;
using Microsoft.ServiceBus;
using MassTransit.AzureServiceBusTransport;

namespace SmartFleet.Core.Infrastructure.MassTransit
{
    public class AzureBusConfig : Module
    {

        public static IBus CreateBus(string queueName)
        {
             var queueUri = "sb://smart-fleet.servicebus.windows.net/" + queueName;

           return Bus.Factory.CreateUsingAzureServiceBus(sbc =>
            {
                var host = sbc.Host(queueUri, h =>
                {
                    h.TokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(
                        ConfigurationManager.AppSettings["AzureSbKeyName"],
                        ConfigurationManager.AppSettings["AzureSbSharedAccessKey"], 
                        TimeSpan.FromDays(1), 
                        TokenScope.Namespace);
                });

                sbc.ReceiveEndpoint(host, ConfigurationManager.AppSettings["ServiceQueueName"], e =>
                {
                    // Configure your consumer(s)
                    //e.Consumer<CheckOrderStatusConsumer>();
                    e.DefaultMessageTimeToLive = TimeSpan.FromMinutes(1);
                    e.EnableDeadLetteringOnMessageExpiration = false;
                });
            });
           

        }

       

        private static string GetConfigValue(string key, string defaultValue)
        {
            string value = ConfigurationManager.AppSettings[key];
            return string.IsNullOrEmpty(value) ? defaultValue : value;
        }
    }
}
