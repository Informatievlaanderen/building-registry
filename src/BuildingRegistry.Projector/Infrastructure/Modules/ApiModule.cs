namespace BuildingRegistry.Projector.Infrastructure.Modules
{
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Autofac;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.LastChangedList;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Autofac;
    using Be.Vlaanderen.Basisregisters.Projector;
    using Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections;
    using Be.Vlaanderen.Basisregisters.Projector.Modules;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using BuildingRegistry.Infrastructure;
    using BuildingRegistry.Projections.Extract;
    using BuildingRegistry.Projections.Extract.BuildingExtract;
    using BuildingRegistry.Projections.Extract.BuildingUnitExtract;
    using BuildingRegistry.Projections.LastChangedList;
    using BuildingRegistry.Projections.Legacy;
    using BuildingRegistry.Projections.Legacy.BuildingDetail;
    using BuildingRegistry.Projections.Legacy.BuildingPersistentIdCrabIdMapping;
    using BuildingRegistry.Projections.Legacy.BuildingSyndication;
    using BuildingRegistry.Projections.Legacy.BuildingUnitDetail;
    using BuildingRegistry.Projections.Legacy.PersistentLocalIdMigration;
    using BuildingRegistry.Projections.Wfs;
    using BuildingRegistry.Projections.Wms;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public class ApiModule : Module
    {
        private const string CatchUpSizesConfigKey = "CatchUpSizes";

        private readonly IConfiguration _configuration;
        private readonly IServiceCollection _services;
        private readonly ILoggerFactory _loggerFactory;

        public ApiModule(
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
            builder.RegisterModule(new DataDogModule(_configuration));

            RegisterProjectionSetup(builder);

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

            RegisterExtractProjections(builder);
            RegisterLastChangedProjections(builder);
            RegisterLegacyProjections(builder);
            RegisterWmsProjections(builder);
            RegisterWfsProjections(builder);
        }

        private void RegisterExtractProjections(ContainerBuilder builder)
        {
            builder
                .RegisterModule(
                    new ExtractModule(
                        _configuration,
                        _services,
                        _loggerFactory));

            builder
                .RegisterProjectionMigrator<ExtractContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)

                .RegisterProjections<BuildingExtractProjections, ExtractContext>(
                    context =>
                        new BuildingExtractProjections(
                            context.Resolve<IOptions<ExtractConfig>>(),
                            DbaseCodePage.Western_European_ANSI.ToEncoding(),
                            WKBReaderFactory.Create()),
                    ConnectedProjectionSettings.Default)

                .RegisterProjections<BuildingUnitExtractProjections, ExtractContext>(
                    context =>
                        new BuildingUnitExtractProjections(
                            context.Resolve<IOptions<ExtractConfig>>(),
                            DbaseCodePage.Western_European_ANSI.ToEncoding(),
                            WKBReaderFactory.Create()),
                    ConnectedProjectionSettings.Default);
        }

        private void RegisterLastChangedProjections(ContainerBuilder builder)
        {
            builder
                .RegisterModule(
                    new LastChangedListModule(
                        _configuration.GetConnectionString("LastChangedList"),
                        _configuration["DataDog:ServiceName"],
                        _services,
                        _loggerFactory));

            builder
                .RegisterProjectionMigrator<BuildingRegistry.Projections.LastChangedList.LastChangedListContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)

                //.RegisterProjections<BuildingProjections, LastChangedListContext>(ConnectedProjectionSettings.Default)
                .RegisterProjections<BuildingUnitProjections, LastChangedListContext>(ConnectedProjectionSettings.Default);
        }

        private void RegisterLegacyProjections(ContainerBuilder builder)
        {
            builder
                .RegisterModule(
                    new LegacyModule(
                        _configuration,
                        _services,
                        _loggerFactory));

            var syndicationCatchUpSize = _configuration.GetSection(CatchUpSizesConfigKey).GetValue<int>("BuildingSyndication");
            var syndicationProjectionSettings =
                ConnectedProjectionSettings.Configure(settings => settings.ConfigureCatchUpPageSize(syndicationCatchUpSize));

            builder
                .RegisterProjectionMigrator<LegacyContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)

                .RegisterProjections<BuildingDetailProjections, LegacyContext>(
                    () => new BuildingDetailProjections(),
                    ConnectedProjectionSettings.Default)
                .RegisterProjections<BuildingSyndicationProjections, LegacyContext>(
                    () => new BuildingSyndicationProjections(),
                    syndicationProjectionSettings)
                .RegisterProjections<BuildingUnitDetailProjections, LegacyContext>(
                    () => new BuildingUnitDetailProjections(),
                    ConnectedProjectionSettings.Default)
                .RegisterProjections<RemovedPersistentLocalIdProjections, LegacyContext>(
                    () => new RemovedPersistentLocalIdProjections(),
                    ConnectedProjectionSettings.Default)
                .RegisterProjections<DuplicatedPersistentLocalIdProjections, LegacyContext>(
                    () => new DuplicatedPersistentLocalIdProjections(),
                    ConnectedProjectionSettings.Default)
                .RegisterProjections<BuildingPersistenLocalIdCrabIdProjections, LegacyContext>(
                    () => new BuildingPersistenLocalIdCrabIdProjections(),
                    ConnectedProjectionSettings.Default);
        }

        private void RegisterWmsProjections(ContainerBuilder builder)
        {
            builder
                .RegisterModule(
                    new WmsModule(
                        _configuration,
                        _services,
                        _loggerFactory));

            var wmsProjectionSettings = ConnectedProjectionSettings
                .Configure(settings =>
                    settings.ConfigureLinearBackoff<SqlException>(_configuration, "Wms"));

            builder
                .RegisterProjectionMigrator<WmsContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)

                .RegisterProjections<BuildingRegistry.Projections.Wms.Building.BuildingProjections, WmsContext>(() =>
                    new BuildingRegistry.Projections.Wms.Building.BuildingProjections(WKBReaderFactory.Create()),
                    wmsProjectionSettings)

                .RegisterProjections<BuildingRegistry.Projections.Wms.BuildingUnit.BuildingUnitProjections, WmsContext>(() =>
                    new BuildingRegistry.Projections.Wms.BuildingUnit.BuildingUnitProjections(WKBReaderFactory.Create()),
                    wmsProjectionSettings);
        }

        private void RegisterWfsProjections(ContainerBuilder builder)
        {
            builder
                .RegisterModule(
                    new WfsModule(
                        _configuration,
                        _services,
                        _loggerFactory));

            var wfsProjectionSettings = ConnectedProjectionSettings
                .Configure(settings =>
                    settings.ConfigureLinearBackoff<SqlException>(_configuration, "Wfs"));

            builder
                .RegisterProjectionMigrator<WfsContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)

                .RegisterProjections<BuildingRegistry.Projections.Wfs.Building.BuildingProjections, WfsContext>(() =>
                        new BuildingRegistry.Projections.Wfs.Building.BuildingProjections(WKBReaderFactory.Create()),
                    wfsProjectionSettings)
                .RegisterProjections<BuildingRegistry.Projections.Wfs.BuildingUnit.BuildingUnitProjections, WfsContext>(() =>
                        new BuildingRegistry.Projections.Wfs.BuildingUnit.BuildingUnitProjections(WKBReaderFactory.Create()),
                    wfsProjectionSettings);
        }
    }
}
