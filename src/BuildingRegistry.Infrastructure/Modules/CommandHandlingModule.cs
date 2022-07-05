namespace BuildingRegistry.Infrastructure.Modules
{
    using System;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Building;
    using Microsoft.Extensions.Configuration;

    public class CommandHandlingModule : Module
    {
        public const string SnapshotIntervalKey = "SnapshotInterval";

        private readonly IConfiguration _configuration;

        public CommandHandlingModule(IConfiguration configuration)
            => _configuration = configuration;

        protected override void Load(ContainerBuilder containerBuilder)
        {
            var value = _configuration[SnapshotIntervalKey] ?? "50";
            var snapshotInterval = Convert.ToInt32(value);

            containerBuilder
                .Register(c => new BuildingFactory(NoSnapshotStrategy.Instance))
                .As<IBuildingFactory>();

            containerBuilder
                .RegisterModule<RepositoriesModule>();

            containerBuilder
                .RegisterType<ConcurrentUnitOfWork>()
                .InstancePerLifetimeScope();

            containerBuilder
                .RegisterEventstreamModule(_configuration);

            Legacy.CommandHandlerModules.Register(containerBuilder);
            CommandHandlerModules.Register(containerBuilder);

            containerBuilder
                .RegisterType<CommandHandlerResolver>()
                .As<ICommandHandlerResolver>();
        }
    }
}
