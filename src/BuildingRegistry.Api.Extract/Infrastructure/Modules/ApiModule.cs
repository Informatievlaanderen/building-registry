namespace BuildingRegistry.Api.Extract.Infrastructure.Modules
{
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Autofac;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Projections.Extract;

    public class ApiModule : Module
    {
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

        protected override void Load(ContainerBuilder containerBuilder)
        {
            var useProjectionsV2ConfigValue = _configuration.GetSection("FeatureToggles")["UseProjectionsV2"];
            var useProjectionsV2 = false;

            if (!string.IsNullOrEmpty(useProjectionsV2ConfigValue))
            {
                useProjectionsV2 = bool.Parse(useProjectionsV2ConfigValue);
            }

            containerBuilder
                .RegisterModule(new MediatRModule(useProjectionsV2))
                .RegisterModule(new DataDogModule(_configuration))
                .RegisterModule(new ExtractModule(_configuration, _services, _loggerFactory, false));

            containerBuilder
                .RegisterType<ProblemDetailsHelper>()
                .AsSelf();

            containerBuilder.Populate(_services);
        }
    }
}