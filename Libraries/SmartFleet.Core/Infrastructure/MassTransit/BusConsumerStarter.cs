using System;
using System.Diagnostics;
using MassTransit;

namespace SmartFleet.Core.Infrastructure.MassTransit
{
    public class BusConsumerStarter 
    {
        /// <summary>
        /// Starts the consumer bus.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queeName">Name of the quee.</param>
        public void StartConsumerBus<T>(string queeName) where T :class ,IConsumer<T>, new()
        {
            try
            {
                MassTransitConfig.ConfigureReceiveBus((cfg, hst) =>
                    cfg.ReceiveEndpoint(hst, queeName, e =>
                        e.Consumer<T>())

                ).Start();
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
                //throw;
            }
        }
    }

  
}