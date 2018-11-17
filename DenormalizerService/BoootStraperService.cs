using System;
using System.Diagnostics;
using Autofac;
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
            throw new System.NotImplementedException();
        }

        public void StartService()
        {
            ContainerBuilder builder = new ContainerBuilder();
            var dependencyRegistrar = new DependencyRegistrar();
            dependencyRegistrar.Register(builder);
            _bus = DependencyRegistrar.ResolveServiceBus();
            try
            {
                _bus.StartAsync();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                _bus.StopAsync();
            }
        }
    }
}