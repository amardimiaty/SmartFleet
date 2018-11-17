using Autofac;
using SmartFleet.Core;
using SmartFleet.Core.Infrastructure.MassTransit;
using TeltonicaService.Handlers;
using TeltonicaService.Infrastucture;

namespace TeltonicaService
{
    public class BootstrapService :IMicorService
    {
        public void StartConsumers(BusConsumerStarter busConsumer)
        {
            //registration of dependencies
            ContainerBuilder builder = new ContainerBuilder();
            var dependencyRegistrar = new DependencyRegistrar();
            dependencyRegistrar.Register(builder);
           
            // start the endpoint consumer
            //  busConsumer.StartConsumerBus<TeltonikaHandler>("Teltonika.endpoint");

        }

        public void StartService()
        {
            throw new System.NotImplementedException();
        }
    }
}
