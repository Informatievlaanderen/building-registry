namespace BuildingRegistry.Tools.Console.Infrastructure.Modules
{
    using Amazon;
    using Amazon.SQS;
    using Api.BackOffice.Abstractions.Building.SqsRequests;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
    using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
    using Be.Vlaanderen.Basisregisters.Sqs;
    using BuildingRegistry.Infrastructure;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using NodaTime;
    using RepairBuilding;
    using SqlStreamStore;
    using TicketingService.Abstractions;
    using TicketingService.Proxy.HttpProxy;
    using SqsQueue = Be.Vlaanderen.Basisregisters.Sqs.SqsQueue;


    public class ToolsModule : Module
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceCollection _services;

        public ToolsModule(
            IConfiguration configuration,
            IServiceCollection services)
        {
            _configuration = configuration;
            _services = services;
        }

        protected override void Load(ContainerBuilder builder)
        {
            RegisterEventHandling(builder);

            builder
                .Register(_ => new SqsOptions(RegionEndpoint.EUWest1, EventsJsonSerializerSettingsProvider.CreateSerializerSettings()))
                .SingleInstance();

            builder.Register(c => new SqsQueue(c.Resolve<SqsOptions>(), _configuration["SqsQueueUrl"]!))
                .As<ISqsQueue>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterInstance(SystemClock.Instance)
                .As<IClock>()
                .SingleInstance();

            builder.RegisterInstance(new RepairBuildingRepository(_configuration.GetConnectionString("Events")!))
                .AsSelf()
                .SingleInstance();

            builder.Register(c => new ProjectionRepository(
                    _configuration.GetConnectionString("Events")!,
                    c.Resolve<IReadonlyStreamStore>()))
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<RepairBuildingSqsHandler>()
                .AsSelf()
                .SingleInstance();

            var sqsRateLimiterConfig = _configuration.GetSection("SqsRateLimit").Get<SqsRateLimiterConfig>();
            builder.Register(_ => sqsRateLimiterConfig!)
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<SqsRateLimiter<RepairBuildingSqsHandler, RepairBuildingSqsRequest>>();

            var ticketingUrl = _configuration["TicketingUrl"]!;
            _services.AddHttpProxyTicketing(ticketingUrl);

            builder
                .Register(_ => new TicketingUrl(ticketingUrl))
                .As<ITicketingUrl>()
                .SingleInstance();

            builder.Populate(_services);
        }

        private void RegisterEventHandling(ContainerBuilder builder)
        {
            builder
                .RegisterModule(
                    new EventHandlingModule(
                        typeof(DomainAssemblyMarker).Assembly,
                        EventsJsonSerializerSettingsProvider.CreateSerializerSettings()))
                .RegisterEventstreamModule(_configuration);
        }
    }
}
