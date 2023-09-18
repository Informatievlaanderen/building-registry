using Amazon.Lambda.Core;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda
{
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Aws.Lambda;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Autofac;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Autofac;
    using MediatR;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using System.Reflection;
    using Abstractions;
    using Abstractions.Building.SqsRequests;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
    using Infrastructure;
    using Infrastructure.Modules;
    using Newtonsoft.Json;
    using TicketingService.Proxy.HttpProxy;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
    using Building;
    using BuildingRegistry.Infrastructure;
    using BuildingRegistry.Infrastructure.Modules;
    using Consumer.Address;
    using Consumer.Read.Parcel;
    using Consumer.Read.Parcel.Infrastructure.Modules;

    public class Function : FunctionBase
    {
        public Function()
            : base(new List<Assembly>{ typeof(PlanBuildingSqsRequest).Assembly })
        { }

        protected override IServiceProvider ConfigureServices(IServiceCollection services)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            var builder = new ContainerBuilder();

            var tempProvider = services.BuildServiceProvider();
            var loggerFactory = tempProvider.GetRequiredService<ILoggerFactory>();

            builder
                .RegisterType<Mediator>()
                .As<IMediator>()
                .InstancePerLifetimeScope();

            builder.RegisterAssemblyTypes(typeof(MessageHandler).GetTypeInfo().Assembly).AsImplementedInterfaces();

            builder.Register(_ => configuration)
                .AsSelf()
                .As<IConfiguration>()
                .SingleInstance();

            services.AddHttpProxyTicketing(configuration.GetSection("TicketingService")["InternalBaseUrl"]);

            // RETRY POLICY
            var maxRetryCount = int.Parse(configuration.GetSection("RetryPolicy")["MaxRetryCount"]);
            var startingDelaySeconds = int.Parse(configuration.GetSection("RetryPolicy")["StartingRetryDelaySeconds"]);

            builder.Register(_ => new LambdaHandlerRetryPolicy(maxRetryCount, startingDelaySeconds))
                .As<ICustomRetryPolicy>()
                .AsSelf()
                .SingleInstance();

            var eventSerializerSettings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

            JsonConvert.DefaultSettings = () => eventSerializerSettings;

            builder
                .RegisterModule(new DataDogModule(configuration))
                .RegisterModule(new SequenceModule(configuration, services, loggerFactory))
                .RegisterModule(new CommandHandlingModule(configuration))
                .RegisterModule(new BackOfficeModule(configuration, services, loggerFactory))
                .RegisterModule(new ConsumerAddressModule(configuration, services, loggerFactory))
                .RegisterModule(new ConsumerParcelModule(configuration, services, loggerFactory));

            builder.RegisterType<IdempotentCommandHandler>()
                .As<IIdempotentCommandHandler>()
                .AsSelf()
                .InstancePerLifetimeScope();

            builder
                .RegisterType<ParcelMatching>()
                .As<IParcelMatching>()
                .InstancePerLifetimeScope();

            services.ConfigureIdempotency(
                configuration.GetSection(IdempotencyConfiguration.Section).Get<IdempotencyConfiguration>()
                    .ConnectionString,
                new IdempotencyMigrationsTableInfo(Schema.Import),
                new IdempotencyTableInfo(Schema.Import),
                loggerFactory);

            builder.Populate(services);

            return new AutofacServiceProvider(builder.Build());
        }
    }
}
