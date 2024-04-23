using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.Json;

[assembly: LambdaSerializer(typeof(JsonSerializer))]
namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda
{
    using System.Reflection;
    using Abstractions;
    using Abstractions.Building.SqsRequests;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Aws.Lambda;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Consumer.Address;
    using Consumer.Read.Parcel.Infrastructure.Modules;
    using GrbAnoApi;
    using Infrastructure;
    using Infrastructure.Modules;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using OrWegwijsApi;
    using TicketingService.Proxy.HttpProxy;

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

            services.AddHttpClient<AnoApiProxy>();
            services.Configure<AnoApiOptions>(configuration.GetSection("AnoApi"));
            builder.RegisterType<AnoApiProxy>()
                .As<IAnoApiProxy>()
                .AsSelf();

            services.AddHttpClient<WegwijsApiProxy>();
            builder.RegisterType<WegwijsApiProxy>()
                .As<IWegwijsApiProxy>()
                .AsSelf();

            builder.Register(_ => configuration)
                .AsSelf()
                .As<IConfiguration>()
                .SingleInstance();

            services.AddHttpProxyTicketing(configuration.GetSection("TicketingService")["InternalBaseUrl"]!);

            var eventSerializerSettings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

            JsonConvert.DefaultSettings = () => eventSerializerSettings;

            builder
                .RegisterModule(new SequenceModule(configuration, services, loggerFactory))
                .RegisterModule(new CommandHandlingModule(configuration))
                .RegisterModule(new BackOfficeModule(configuration, services, loggerFactory))
                .RegisterModule(new ConsumerAddressModule(configuration, services, loggerFactory))
                .RegisterModule(new ConsumerParcelModule(configuration, services, loggerFactory))
                .RegisterModule(new LambdaModule(configuration, services));

            services.ConfigureIdempotency(
                configuration
                    .GetSection(IdempotencyConfiguration.Section)
                    .Get<IdempotencyConfiguration>()!
                    .ConnectionString!,
                new IdempotencyMigrationsTableInfo(Schema.Import),
                new IdempotencyTableInfo(Schema.Import),
                loggerFactory);

            builder.Populate(services);

            return new AutofacServiceProvider(builder.Build());
        }
    }


}
