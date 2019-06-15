using System;
using System.Diagnostics;
using Autofac;
using DenormalizerService.Handler;
using DenormalizerService.Infrastructure;
using MassTransit;
using SmartFleet.Core;
using SmartFleet.Core.Infrastructure.MassTransit;

namespace DenormalizerService
{
    public class BoootStraperService :IMicorService
    {
        private IBusControl _bus;

        public void StartConsumers(BusConsumerStarter busConsumer)
        {
            
        }

        public void StartService()
        {
            ContainerBuilder builder = new ContainerBuilder();
            var dependencyRegistrar = new DependencyRegistrar();
            dependencyRegistrar.Register(builder);
           
            try
            {
                MassTransitConfig.ConfigureReceiveBus((cfg, hst) =>
                    cfg.ReceiveEndpoint(hst, "Teltonika.endpoint", e =>
                        e.Consumer<DenormalizerHandler>())

                ).Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
              //  throw;
            }
        }
    }
}