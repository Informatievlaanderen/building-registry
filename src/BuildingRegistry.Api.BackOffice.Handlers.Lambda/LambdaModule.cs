namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda
{
    using System.Reflection;
    using Amazon;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.SqlServer.MigrationExtensions;
    using Be.Vlaanderen.Basisregisters.Sqs;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
    using Building;
    using Infrastructure;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using AutofacModule = Autofac.Module;
    using SqsQueue = Be.Vlaanderen.Basisregisters.Sqs.SqsQueue;

    public class LambdaModule : AutofacModule
    {
        private readonly IConfiguration _configuration;

        public LambdaModule(
            IConfiguration configuration,
            IServiceCollection services)
        {
            _configuration = configuration;
            services
                .AddDbContext<BuildingGeometryContext>((serviceProvider, options) => options
                    .UseLoggerFactory(serviceProvider.GetRequiredService<ILoggerFactory>())
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                    .UseSqlServer(
                        serviceProvider.GetRequiredService<IConfiguration>().GetConnectionString("Legacy"),
                        sqlServerOptions =>
                        {
                            sqlServerOptions.EnableRetryOnFailure();
                            sqlServerOptions.MigrationsHistoryTable(MigrationTables.Legacy, Schema.Legacy);
                            sqlServerOptions.UseNetTopologySuite();
                        })
                    .UseExtendedSqlServerMigrations());

            services.AddScoped<IBuildingGeometries>(serviceProvider =>
            {
                var enabled = serviceProvider.GetRequiredService<IConfiguration>().GetValue("OverlapValidationToggle", true);

                return enabled
                    ? serviceProvider.GetRequiredService<BuildingGeometryContext>()
                    : new NoOverlappingBuildingGeometries();
            });
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<Mediator>()
                .As<IMediator>()
                .InstancePerLifetimeScope();

            builder.RegisterAssemblyTypes(typeof(MessageHandler).GetTypeInfo().Assembly).AsImplementedInterfaces();

            // RETRY POLICY
            var maxRetryCount = int.Parse(_configuration.GetSection("RetryPolicy")["MaxRetryCount"]!);
            var startingDelaySeconds = int.Parse(_configuration.GetSection("RetryPolicy")["StartingRetryDelaySeconds"]!);

            builder.Register(_ => new LambdaHandlerRetryPolicy(maxRetryCount, startingDelaySeconds))
                .As<ICustomRetryPolicy>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<IdempotentCommandHandler>()
                .As<IIdempotentCommandHandler>()
                .AsSelf()
                .InstancePerLifetimeScope();

            builder
                .RegisterType<ParcelMatching>()
                .As<IParcelMatching>()
                .InstancePerLifetimeScope();

            builder
                .Register(_ => new SqsOptions(RegionEndpoint.EUWest1, EventsJsonSerializerSettingsProvider.CreateSerializerSettings()))
                .SingleInstance();

            builder.Register(c =>
                    new SqsQueue(c.Resolve<SqsOptions>(), _configuration.GetSection("AnoApi").GetValue<string>("SqsUrl") ?? throw new ArgumentNullException("SqsUrl")))
                .As<ISqsQueue>()
                .AsSelf()
                .SingleInstance();
        }
    }
}
