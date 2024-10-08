namespace BuildingRegistry.Projections.LastChangedList.Console.Infrastructure.Modules
{
    using System;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.LastChangedList;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Autofac;
    using Be.Vlaanderen.Basisregisters.Projector;
    using Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections;
    using Be.Vlaanderen.Basisregisters.Projector.Modules;
    using BuildingRegistry.Infrastructure;
    using BuildingRegistry.Projections.Legacy.BuildingUnitDetailV2WithCount;
    using Legacy.BuildingDetailV2;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using LastChangedListContextMigrationFactory = LastChangedListContextMigrationFactory;

    public class LastChangedListConsoleModule : Module
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceCollection _services;
        private readonly ILoggerFactory _loggerFactory;

        public LastChangedListConsoleModule(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _services = services;
            _loggerFactory = loggerFactory;
        }

        protected override void Load(ContainerBuilder builder)
        {
            RegisterProjectionSetup(builder);

            builder
                .RegisterType<ProblemDetailsHelper>()
                .AsSelf();

            builder.Populate(_services);
        }

        private void RegisterProjectionSetup(ContainerBuilder builder)
        {
            builder
                .RegisterModule(
                    new EventHandlingModule(
                        typeof(DomainAssemblyMarker).Assembly,
                        EventsJsonSerializerSettingsProvider.CreateSerializerSettings()))
                .RegisterModule<EnvelopeModule>()
                .RegisterEventstreamModule(_configuration)
                .RegisterModule(new ProjectorModule(_configuration));

            RegisterProjections(builder);
        }

        private void RegisterProjections(ContainerBuilder builder)
        {
            var logger = _loggerFactory.CreateLogger<LastChangedListConsoleModule>();
            var connectionString = _configuration.GetConnectionString("LastChangedList");

            builder.RegisterModule(new LastChangedListModule(connectionString, _services, _loggerFactory));

            logger.LogInformation(
                "Added {Context} to services:" +
                Environment.NewLine +
                "\tSchema: {Schema}" +
                Environment.NewLine +
                "\tTableName: {TableName}",
                nameof(LastChangedListContext), LastChangedListContext.Schema, LastChangedListContext.MigrationsHistoryTable);

            builder
                .Register(c =>
                    new LastChangedListBuildingCacheValidator(
                        _configuration.GetConnectionString("LegacyProjections"),
                        BuildingDetailItemConfiguration.ProjectionStateName))
                .AsSelf();
            builder
                .Register(c =>
                    new LastChangedListBuildingUnitCacheValidator(
                        _configuration.GetConnectionString("LegacyProjections"),
                        BuildingUnitDetailItemConfiguration.ProjectionStateName))
                .AsSelf();

            builder
                .RegisterProjectionMigrator<LastChangedListContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)
                .RegisterProjectionMigrator<DataMigrationContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)
                .RegisterProjections<BuildingProjections, LastChangedListContext>(
                    context => new BuildingProjections(context.Resolve<LastChangedListBuildingCacheValidator>()),
                    ConnectedProjectionSettings.Default)
                .RegisterProjections<BuildingUnitProjections, LastChangedListContext>(
                    context => new BuildingUnitProjections(context.Resolve<LastChangedListBuildingUnitCacheValidator>()),
                    ConnectedProjectionSettings.Default);
        }
    }
}
