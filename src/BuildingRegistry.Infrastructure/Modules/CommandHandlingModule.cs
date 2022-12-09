namespace BuildingRegistry.Infrastructure.Modules
{
    using System;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Building;
    using Microsoft.Extensions.Configuration;

    //TODO: split for backoffice & migrator VS crabimport
    public class CommandHandlingModule : Module
    {
        public const string SnapshotIntervalKey = "SnapshotInterval";

        private readonly IConfiguration _configuration;

        public CommandHandlingModule(IConfiguration configuration)
            => _configuration = configuration;

        protected override void Load(ContainerBuilder builder)
        {
            var value = _configuration[SnapshotIntervalKey] ?? "50";
            var snapshotInterval = Convert.ToInt32(value);

            ISnapshotStrategy snapshotStrategy = NoSnapshotStrategy.Instance;
            if (snapshotInterval > 0)
            {
                snapshotStrategy = IntervalStrategy.SnapshotEvery(snapshotInterval);
            }
            
            builder
                .Register(c => new BuildingFactory(snapshotStrategy, c.Resolve<IAddCommonBuildingUnit>(), c.Resolve<IAddresses>()))
                .As<IBuildingFactory>();

            builder
                .RegisterModule<RepositoriesModule>();

            builder
                .RegisterType<ConcurrentUnitOfWork>()
                .InstancePerLifetimeScope();

            builder
                .RegisterEventstreamModule(_configuration);

            Legacy.CommandHandlerModules.Register(builder);
            CommandHandlerModules.Register(builder);

            builder
                .RegisterType<CommandHandlerResolver>()
                .As<ICommandHandlerResolver>();
        }
    }
}
