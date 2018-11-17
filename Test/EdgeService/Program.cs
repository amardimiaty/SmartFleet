using Autofac;
using EdgeService.Handler;
using SmartFleet.Core;
using SmartFleet.Core.Data;
using SmartFleet.Core.Infrastructure.MassTransit;
using SmartFleet.Data;
using SmartFleet.Data.Dbcontextccope.Implementations;

namespace EdgeService
{
    class Program
    {
        private static IContainer Container { get; set; }

        static void Main(string[] args)
        {
            var busConfig = new BusConsumerStarter();
            var builder = new ContainerBuilder();
            builder.RegisterType<DbContextScopeFactory>().As<IDbContextScopeFactory>();
            builder.RegisterType<AmbientDbContextLocator>().As<IAmbientDbContextLocator>();
            builder.RegisterGeneric(typeof(EfScopeRepository<>)).As(typeof(IScopeRepository<>)).InstancePerLifetimeScope();
            Container = builder.Build();
         
            Container.Resolve<IAmbientDbContextLocator>();
          
          //  busConfig.StartConsumerBus<TeltonikaHandler>("Teltonika.endpoint");
            //busConfig.StartConsumerBus<Tk103Handler>("Tk1003.endpoint");

        }

        public static IScopeRepository<T> ScopeRepository<T>() where T: BaseEntity
        {
            return Container.Resolve<IScopeRepository<T>>();

        }

        public static IDbContextScopeFactory ResolveDbContextScopeFactory()
        {
            return Container.Resolve<IDbContextScopeFactory>();
        }


    }
}
