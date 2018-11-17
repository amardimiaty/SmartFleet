using System;
using System.Configuration;
using Autofac;
using DenormalizerService.Handler;
using MassTransit;
using MassTransit.AzureServiceBusTransport;
using Microsoft.ServiceBus;
using SmartFleet.Core.Data;
using SmartFleet.Core.Infrastructure.Registration;
using SmartFleet.Data;
using SmartFleet.Data.Dbcontextccope.Implementations;

namespace DenormalizerService.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        private static IContainer Container { get; set; }

        public void Register(ContainerBuilder builder)
        {

            builder.Register(context =>
                {
                    return Bus.Factory.CreateUsingAzureServiceBus(sbc =>
                    {
                        var serviceUri = ServiceBusEnvironment.CreateServiceUri("sb",
                            ConfigurationManager.AppSettings["AzureSbNamespace"],
                            ConfigurationManager.AppSettings["AzureSbPath"]);

                        var host = ServiceBusBusFactoryConfiguratorExtensions.Host(sbc, serviceUri,
                            h =>
                            {
                                h.TokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(
                                    ConfigurationManager.AppSettings["AzureSbKeyName"],
                                    ConfigurationManager.AppSettings["AzureSbSharedAccessKey"], TimeSpan.FromDays(1),
                                    TokenScope.Namespace);
                            });

                        sbc.ReceiveEndpoint(host, "denormalizer.endpoint", e =>
                        {
                            // Configure your consumer(s)
                            ConsumerExtensions.Consumer<DenormalizerHandler>(e);
                            e.DefaultMessageTimeToLive = TimeSpan.FromMinutes(1);
                            e.EnableDeadLetteringOnMessageExpiration = false;
                        });
                    });
                })
                .SingleInstance()
                .As<IBusControl>()
                .As<IBus>();
            //builder.RegisterType<SmartFleetObjectContext>().As<SmartFleetObjectContext>();
            builder.RegisterType<DbContextScopeFactory>().As<IDbContextScopeFactory>();

            Container = builder.Build();
        }

        public static IBusControl ResolveServiceBus()
        {
            return Container.Resolve<IBusControl>();
        }
        public static IDbContextScopeFactory ResolveDbContextScopeFactory()
        {
            return Container.Resolve<IDbContextScopeFactory>();
        }
    }
    
}
