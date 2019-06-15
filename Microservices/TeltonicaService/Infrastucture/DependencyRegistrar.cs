using System;
using Autofac;
using SmartFleet.Core;
using SmartFleet.Core.Data;
using SmartFleet.Core.Infrastructure.Registration;
using SmartFleet.Data;
using SmartFleet.Data.Dbcontextccope.Implementations;

namespace TeltonicaService.Infrastucture
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        private static IContainer Container { get; set; }

        public void Register(ContainerBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            // registration of dependencies
            builder.RegisterType<DbContextScopeFactory>().As<IDbContextScopeFactory>();
            builder.RegisterType<AmbientDbContextLocator>().As<IAmbientDbContextLocator>();
            builder.RegisterGeneric(typeof(EfScopeRepository<>)).As(typeof(IScopeRepository<>))
                .InstancePerLifetimeScope();
            Container = builder.Build();

            Container.Resolve<IAmbientDbContextLocator>();
        }

        public static IScopeRepository<T> ScopeRepository<T>() where T : BaseEntity
        {
            return Container.Resolve<IScopeRepository<T>>();

        }
        public static IDbContextScopeFactory ResolveDbContextScopeFactory()
        {
            return Container.Resolve<IDbContextScopeFactory>();
        }

       
    }
}
