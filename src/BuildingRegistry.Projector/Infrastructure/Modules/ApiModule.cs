namespace BuildingRegistry.Projector.Infrastructure.Modules
{
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Autofac;
    using Be.Vlaanderen.Basisregisters.Projector;
    using Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections;
    using Be.Vlaanderen.Basisregisters.Projector.Modules;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using BuildingRegistry.Infrastructure;
    using BuildingRegistry.Projections.Extract;
    using BuildingRegistry.Projections.Extract.BuildingExtract;
    using BuildingRegistry.Projections.Extract.BuildingUnitAddressLinkExtractWithCount;
    using BuildingRegistry.Projections.Extract.BuildingUnitExtract;
    using BuildingRegistry.Projections.Integration;
    using BuildingRegistry.Projections.Integration.Building.LatestItem;
    using BuildingRegistry.Projections.Integration.Building.Version;
    using BuildingRegistry.Projections.Integration.BuildingUnit.LatestItem;
    using BuildingRegistry.Projections.Integration.Infrastructure;
    using BuildingRegistry.Projections.LastChangedList;
    using BuildingRegistry.Projections.Legacy;
    using BuildingRegistry.Projections.Legacy.BuildingDetailV2;
    using BuildingRegistry.Projections.Legacy.BuildingSyndicationWithCount;
    using BuildingRegistry.Projections.Legacy.BuildingUnitDetailV2WithCount;
    using BuildingRegistry.Projections.Wfs;
    using BuildingRegistry.Projections.Wms;
    using BuildingRegistry.Projections.Wms.BuildingUnitV2;
    using BuildingRegistry.Projections.Wms.BuildingV2;
    using BuildingRegistry.Projections.Wms.BuildingV3;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using BuildingVersionFromMigration = BuildingRegistry.Projections.Integration.Building.VersionFromMigration.BuildingVersionProjections;

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

            RegisterLastChangedProjections(builder);

            RegisterExtractProjectionsV2(builder);
            RegisterLegacyProjectionsV2(builder);
            RegisterWmsProjectionsV2(builder);
            RegisterWfsProjectionsV2(builder);

            if (_configuration.GetSection("Integration").GetValue("Enabled", false))
            {
                RegisterIntegrationProjections(builder);
            }
        }

        private void RegisterExtractProjectionsV2(ContainerBuilder builder)
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
                .RegisterProjections<BuildingExtractV2Projections, ExtractContext>(
                    context =>
                        new BuildingExtractV2Projections(
                            context.Resolve<IOptions<ExtractConfig>>(),
                            DbaseCodePage.Western_European_ANSI.ToEncoding(),
                            WKBReaderFactory.Create()),
                    ConnectedProjectionSettings.Default)
                .RegisterProjections<BuildingExtractV2EsriProjections, ExtractContext>(
                    context =>
                        new BuildingExtractV2EsriProjections(
                            context.Resolve<IOptions<ExtractConfig>>(),
                            DbaseCodePage.Western_European_ANSI.ToEncoding(),
                            WKBReaderFactory.Create()),
                    ConnectedProjectionSettings.Default)
                .RegisterProjections<BuildingUnitExtractV2Projections, ExtractContext>(
                    context =>
                        new BuildingUnitExtractV2Projections(
                            context.Resolve<IOptions<ExtractConfig>>(),
                            DbaseCodePage.Western_European_ANSI.ToEncoding(),
                            WKBReaderFactory.Create()),
                    ConnectedProjectionSettings.Default)
                .RegisterProjections<BuildingUnitAddressLinkExtractProjections, ExtractContext>(
                    context => new BuildingUnitAddressLinkExtractProjections(DbaseCodePage.Western_European_ANSI.ToEncoding()),
                    ConnectedProjectionSettings.Default)
                ;
        }

        private void RegisterLastChangedProjections(ContainerBuilder builder)
        {
            builder
                .RegisterModule(
                    new BuildingUnitLastChangedListModule(
                        _configuration.GetConnectionString("LastChangedList")!,
                        _services,
                        _loggerFactory));
        }

        private void RegisterLegacyProjectionsV2(ContainerBuilder builder)
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
                .RegisterProjections<BuildingDetailV2Projections, LegacyContext>(
                    () => new BuildingDetailV2Projections(),
                    ConnectedProjectionSettings.Default)
                .RegisterProjections<BuildingSyndicationProjections, LegacyContext>(
                    () => new BuildingSyndicationProjections(),
                    syndicationProjectionSettings)
                .RegisterProjections<BuildingUnitDetailV2Projections, LegacyContext>(
                    () => new BuildingUnitDetailV2Projections(),
                    ConnectedProjectionSettings.Default)
                ;
        }

        private void RegisterWmsProjectionsV2(ContainerBuilder builder)
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
                .RegisterProjections<BuildingV2Projections, WmsContext>(() =>
                        new BuildingV2Projections(WKBReaderFactory.Create()),
                    wmsProjectionSettings)
                .RegisterProjections<BuildingV3Projections, WmsContext>(() =>
                        new BuildingV3Projections(WKBReaderFactory.Create()),
                    wmsProjectionSettings)
                .RegisterProjections<BuildingUnitV2Projections, WmsContext>(() =>
                        new BuildingUnitV2Projections(WKBReaderFactory.Create()),
                    wmsProjectionSettings);
        }

        private void RegisterWfsProjectionsV2(ContainerBuilder builder)
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
                .RegisterProjections<BuildingRegistry.Projections.Wfs.BuildingV2.BuildingV2Projections, WfsContext>(() =>
                        new BuildingRegistry.Projections.Wfs.BuildingV2.BuildingV2Projections(WKBReaderFactory.Create()),
                    wfsProjectionSettings)
                .RegisterProjections<BuildingRegistry.Projections.Wfs.BuildingV3.BuildingV3Projections, WfsContext>(() =>
                        new BuildingRegistry.Projections.Wfs.BuildingV3.BuildingV3Projections(WKBReaderFactory.Create()),
                    wfsProjectionSettings)
                .RegisterProjections<BuildingRegistry.Projections.Wfs.BuildingUnitV2.BuildingUnitV2Projections, WfsContext>(() =>
                        new BuildingRegistry.Projections.Wfs.BuildingUnitV2.BuildingUnitV2Projections(WKBReaderFactory.Create()),
                    wfsProjectionSettings);
        }

        private void RegisterIntegrationProjections(ContainerBuilder builder)
        {
            builder
                .RegisterModule(
                    new IntegrationModule(
                        _configuration,
                        _services,
                        _loggerFactory));

            var buildingIntegrationCatchUpSize = _configuration.GetSection(CatchUpSizesConfigKey).GetValue<int>("BuildingIntegration");
            var buildingProjectionSettings =
                ConnectedProjectionSettings.Configure(settings => settings.ConfigureCatchUpPageSize(buildingIntegrationCatchUpSize));

            builder
                .RegisterProjectionMigrator<IntegrationContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)
                .RegisterProjections<BuildingLatestItemProjections, IntegrationContext>(
                    context => new BuildingLatestItemProjections(context.Resolve<IOptions<IntegrationOptions>>()),
                    ConnectedProjectionSettings.Default)
                .RegisterProjections<BuildingUnitLatestItemProjections, IntegrationContext>(
                    context => new BuildingUnitLatestItemProjections(context.Resolve<IOptions<IntegrationOptions>>()),
                    ConnectedProjectionSettings.Default)
                .RegisterProjections<BuildingVersionProjections, IntegrationContext>(
                    context => new BuildingVersionProjections(
                        context.Resolve<IOptions<IntegrationOptions>>(),
                        context.Resolve<IPersistentLocalIdFinder>(),
                        context.Resolve<IAddresses>()),
                    buildingProjectionSettings)
                .RegisterProjections<BuildingVersionProjections, IntegrationContext>(
                    context => new BuildingVersionProjections(
                        context.Resolve<IOptions<IntegrationOptions>>(),
                        context.Resolve<IPersistentLocalIdFinder>(),
                        context.Resolve<IAddresses>()),
                    buildingProjectionSettings)
                .RegisterProjections<BuildingVersionFromMigration, IntegrationContext>(
                    context => new BuildingVersionFromMigration(
                        context.Resolve<IOptions<IntegrationOptions>>()),
                    buildingProjectionSettings)
                ;
        }
    }
}
