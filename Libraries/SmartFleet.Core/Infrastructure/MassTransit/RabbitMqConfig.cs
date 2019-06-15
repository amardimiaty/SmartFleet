using System;
using System.Configuration;
using MassTransit;
using MassTransit.RabbitMqTransport;

namespace SmartFleet.Core.Infrastructure.MassTransit
{
    public static class RabbitMqConfig
    {
        static string url = ConfigurationManager.AppSettings["RabbitQueueFullUri"];

        public static IBusControl InitReceiverBus<T>(string endpoint) where T : class, IConsumer, new()
        {
            return Bus.Factory.CreateUsingRabbitMq(sbc =>
            {
                IRabbitMqHost host = sbc.Host(
                    new Uri(url.Replace("amqp://", "rabbitmq://")),
                    hst =>
                    {
                        hst.Username(ConfigurationManager.AppSettings["RabbitUsername"]);
                        hst.Password(ConfigurationManager.AppSettings["RabbitPassword"]);
                    });

                sbc.ReceiveEndpoint(host, endpoint, e =>
                {
                    // Configure your consumer(s)
                    ConsumerExtensions.Consumer<T>(e);
                });
            });
       
        }

    }
}
