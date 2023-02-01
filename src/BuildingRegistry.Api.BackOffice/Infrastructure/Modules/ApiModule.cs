namespace BuildingRegistry.Api.BackOffice.Infrastructure.Modules
{
    using Abstractions;
    using Abstractions.Building.Validators;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Microsoft;
    using Be.Vlaanderen.Basisregisters.DependencyInjection;
    using BuildingRegistry.Infrastructure;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Consumer.Address;
    using Be.Vlaanderen.Basisregisters.AcmIdm;

    public class ApiModule : Module
    {
        internal const string SqsQueueUrlConfigKey = "SqsQueueUrl";

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
            _services.RegisterModule(new DataDogModule(_configuration));

            builder
                .RegisterType<ProblemDetailsHelper>()
                .AsSelf();

            builder
                .RegisterType<IfMatchHeaderValidator>()
                .As<IIfMatchHeaderValidator>()
                .AsSelf();

            builder
                .RegisterType<BuildingExistsValidator>()
                .AsSelf()
                .InstancePerLifetimeScope();

            builder
                .RegisterModule(new BackOfficeModule(_configuration, _services, _loggerFactory))
                .RegisterModule(new MediatRModule())
                .RegisterModule(new AggregateSourceModule(_configuration))
                .RegisterModule(new SqsHandlersModule(_configuration[SqsQueueUrlConfigKey]))
                .RegisterModule(new TicketingModule(_configuration, _services))
                .RegisterModule(new ConsumerAddressModule(_configuration, _services, _loggerFactory))
                // Required because backoffice is responsible for running EF migrations
                .RegisterModule(new IdempotencyModule(
                    _services,
                    _configuration.GetSection(IdempotencyConfiguration.Section).Get<IdempotencyConfiguration>()
                        .ConnectionString,
                    new IdempotencyMigrationsTableInfo(Schema.Import),
                    new IdempotencyTableInfo(Schema.Import),
                    _loggerFactory));

            _services.AddAcmIdmAuthorizationHandlers();

            builder.Populate(_services);
        }
    }
}
