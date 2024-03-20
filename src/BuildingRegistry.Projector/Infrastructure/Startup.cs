namespace BuildingRegistry.Projector.Infrastructure
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Asp.Versioning.ApiExplorer;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Autofac;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.LastChangedList;
    using Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections;
    using BuildingRegistry.Projections.Extract;
    using BuildingRegistry.Projections.Integration.Infrastructure;
    using BuildingRegistry.Projections.Legacy;
    using BuildingRegistry.Projections.Wfs;
    using BuildingRegistry.Projections.Wms;
    using Configuration;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.OpenApi.Models;
    using Modules;

    /// <summary>Represents the startup process for the application.</summary>
    public class Startup
    {
        private const string DatabaseTag = "db";

        private IContainer _applicationContainer;

        private readonly IConfiguration _configuration;
        private readonly ILoggerFactory _loggerFactory;
        private readonly CancellationTokenSource _projectionsCancellationTokenSource = new CancellationTokenSource();

        public Startup(
            IConfiguration configuration,
            ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _loggerFactory = loggerFactory;
        }

        /// <summary>Configures services for the application.</summary>
        /// <param name="services">The collection of services to configure the application with.</param>
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            var baseUrl = _configuration.GetValue<string>("BaseUrl");
            var baseUrlForExceptions = baseUrl.EndsWith("/")
                ? baseUrl.Substring(0, baseUrl.Length - 1)
                : baseUrl;

            services
                .ConfigureDefaultForApi<Startup>(new StartupConfigureOptions
                {
                    Cors =
                    {
                        Origins = _configuration
                            .GetSection("Cors")
                            .GetChildren()
                            .Select(c => c.Value)
                            .ToArray()
                    },
                    Server =
                    {
                        BaseUrl = baseUrlForExceptions
                    },
                    Swagger =
                    {
                        ApiInfo = (provider, description) => new OpenApiInfo
                        {
                            Version = description.ApiVersion.ToString(),
                            Title = "Basisregisters Vlaanderen Building Registry API",
                            Description = GetApiLeadingText(description),
                            Contact = new OpenApiContact
                            {
                                Name = "Digitaal Vlaanderen",
                                Email = "digitaal.vlaanderen@vlaanderen.be",
                                Url = new Uri("https://legacy.basisregisters.vlaanderen")
                            }
                        },
                        XmlCommentPaths = new[] {typeof(Startup).GetTypeInfo().Assembly.GetName().Name}
                    },
                    MiddlewareHooks =
                    {
                        FluentValidation = fv => fv.RegisterValidatorsFromAssemblyContaining<Startup>(),

                        AfterHealthChecks = health =>
                        {
                            var connectionStrings = _configuration
                                .GetSection("ConnectionStrings")
                                .GetChildren()
                                .ToList();

                            if (!_configuration.GetSection("Integration").GetValue("Enabled", false))
                            {
                                connectionStrings = connectionStrings
                                    .Where(x => !x.Key.StartsWith("Integration", StringComparison.OrdinalIgnoreCase))
                                    .ToList();
                            }

                            foreach (var connectionString in connectionStrings
                                         .Where(x => !x.Value.Contains("host", StringComparison.OrdinalIgnoreCase)))
                            {
                                health.AddSqlServer(
                                    connectionString.Value,
                                    name: $"sqlserver-{connectionString.Key.ToLowerInvariant()}",
                                    tags: new[] {DatabaseTag, "sql", "sqlserver"});
                            }

                            foreach (var connectionString in connectionStrings
                                         .Where(x => x.Value.Contains("host", StringComparison.OrdinalIgnoreCase)))
                            {
                                health.AddNpgSql(
                                    connectionString.Value,
                                    name: $"npgsql-{connectionString.Key.ToLowerInvariant()}",
                                    tags: new[] {DatabaseTag, "sql", "npgsql"});
                            }

                            health.AddDbContextCheck<ExtractContext>(
                                $"dbcontext-{nameof(ExtractContext).ToLowerInvariant()}",
                                tags: new[] {DatabaseTag, "sql", "sqlserver"});

                            health.AddDbContextCheck<LastChangedListContext>(
                                $"dbcontext-{nameof(LastChangedListContext).ToLowerInvariant()}",
                                tags: new[] {DatabaseTag, "sql", "sqlserver"});

                            health.AddDbContextCheck<LegacyContext>(
                                $"dbcontext-{nameof(LegacyContext).ToLowerInvariant()}",
                                tags: new[] {DatabaseTag, "sql", "sqlserver"});

                            health.AddDbContextCheck<WmsContext>(
                                $"dbcontext-{nameof(WmsContext).ToLowerInvariant()}",
                                tags: new[] {DatabaseTag, "sql", "sqlserver"});

                            health.AddDbContextCheck<WfsContext>(
                                $"dbcontext-{nameof(WfsContext).ToLowerInvariant()}",
                                tags: new[] {DatabaseTag, "sql", "sqlserver"});
                        }
                    }
                })
                .Configure<ExtractConfig>(_configuration.GetSection("Extract"))
                .Configure<IntegrationOptions>(_configuration.GetSection("Integration"))
                .Configure<FeatureToggleOptions>(_configuration.GetSection(FeatureToggleOptions.ConfigurationKey))
                .AddSingleton(c => new UseProjectionsV2Toggle(c.GetRequiredService<IOptions<FeatureToggleOptions>>().Value.UseProjectionsV2));

            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterModule(new LoggingModule(_configuration, services));
            containerBuilder.RegisterModule(new ApiModule(_configuration, services, _loggerFactory));
            _applicationContainer = containerBuilder.Build();

            return new AutofacServiceProvider(_applicationContainer);
        }

        public void Configure(
            IServiceProvider serviceProvider,
            IApplicationBuilder app,
            IWebHostEnvironment env,
            IHostApplicationLifetime appLifetime,
            ILoggerFactory loggerFactory,
            IApiVersionDescriptionProvider apiVersionProvider,
            ApiDataDogToggle datadogToggle,
            ApiDebugDataDogToggle debugDataDogToggle,
            HealthCheckService healthCheckService)
        {
            StartupHelpers.CheckDatabases(healthCheckService, DatabaseTag, loggerFactory).GetAwaiter().GetResult();

            app
                .UseDataDog<Startup>(new DataDogOptions
                {
                    Common =
                    {
                        ServiceProvider = serviceProvider,
                        LoggerFactory = loggerFactory
                    },
                    Toggles =
                    {
                        Enable = datadogToggle,
                        Debug = debugDataDogToggle
                    },
                    Tracing =
                    {
                        ServiceName = _configuration["DataDog:ServiceName"],
                    }
                })
                .UseDefaultForApi(new StartupUseOptions
                {
                    Common =
                    {
                        ApplicationContainer = _applicationContainer,
                        ServiceProvider = serviceProvider,
                        HostingEnvironment = env,
                        ApplicationLifetime = appLifetime,
                        LoggerFactory = loggerFactory,
                    },
                    Api =
                    {
                        VersionProvider = apiVersionProvider,
                        Info = groupName => $"Basisregisters.Vlaanderen - Building Information Registry API {groupName}",
                        CSharpClientOptions =
                        {
                            ClassName = "BuildingRegistryProjector",
                            Namespace = "Be.Vlaanderen.Basisregisters"
                        },
                        TypeScriptClientOptions =
                        {
                            ClassName = "BuildingRegistryProjector"
                        }
                    },
                    Server =
                    {
                        PoweredByName = "Vlaamse overheid - Basisregisters Vlaanderen",
                        ServerName = "Digitaal Vlaanderen"
                    },
                    MiddlewareHooks =
                    {
                        AfterMiddleware = x => x.UseMiddleware<AddNoCacheHeadersMiddleware>()
                    }
                });

            appLifetime.ApplicationStopping.Register(() => _projectionsCancellationTokenSource.Cancel());
            appLifetime.ApplicationStarted.Register(() =>
            {
                var projectionsManager = _applicationContainer.Resolve<IConnectedProjectionsManager>();
                Task.Run(async () =>
                    await projectionsManager.Resume(_projectionsCancellationTokenSource.Token).ConfigureAwait(false)
                ).ConfigureAwait(false);
            });
        }

        private static string GetApiLeadingText(ApiVersionDescription description)
            => $"Momenteel leest u de documentatie voor versie {description.ApiVersion} van de Basisregisters Vlaanderen Building Registry API{string.Format(description.IsDeprecated ? ", **deze API versie is niet meer ondersteund * *." : ".")}";
    }
}
