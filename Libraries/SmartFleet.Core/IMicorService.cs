
using SmartFleet.Core.Infrastructure.MassTransit;

namespace SmartFleet.Core
{
    public interface IMicorService
    {
        void StartConsumers(BusConsumerStarter busConsumer);
        void StartService();
    }
}