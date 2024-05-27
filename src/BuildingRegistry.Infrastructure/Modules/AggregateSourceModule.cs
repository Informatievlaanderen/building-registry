namespace BuildingRegistry.Infrastructure.Modules
{
    using System;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
    using BuildingRegistry.AllStream;
    using BuildingRegistry.Building;
    using BuildingRegistry.Infrastructure;
    using BuildingRegistry.Infrastructure.Repositories;
    using Microsoft.Extensions.Configuration;

    public class AggregateSourceModule : Module
    {
        public const string SnapshotIntervalKey = "SnapshotInterval";

        private readonly IConfiguration _configuration;

        public AggregateSourceModule(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var eventSerializerSettings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

            var value = _configuration[SnapshotIntervalKey] ?? "50";
            var snapshotInterval = Convert.ToInt32(value);

            ISnapshotStrategy snapshotStrategy = NoSnapshotStrategy.Instance;
            if (snapshotInterval > 0)
            {
                snapshotStrategy = IntervalStrategy.SnapshotEvery(snapshotInterval);
            }

            builder.RegisterSnapshotModule(_configuration);

            builder
                .Register(c => new BuildingFactory(snapshotStrategy))
                .As<IBuildingFactory>();

            builder
                .RegisterType<Buildings>()
                .As<IBuildings>();

            builder
                .RegisterType<AllStreamRepository>()
                .As<IAllStreamRepository>();

            builder
                .RegisterModule(new EventHandlingModule(typeof(DomainAssemblyMarker).Assembly, eventSerializerSettings));

            builder.RegisterEventstreamModule(_configuration);

            builder
                .RegisterType<ConcurrentUnitOfWork>()
                .InstancePerLifetimeScope();
        }
    }
}
