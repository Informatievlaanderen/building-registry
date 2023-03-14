namespace BuildingRegistry.Api.Grb.Infrastructure.Modules
{
    using Autofac;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using TicketingService.Abstractions;
    using TicketingService.Proxy.HttpProxy;

    public class TicketingModule : Module
    {
        private readonly IConfiguration _configuration;
        internal const string TicketingServiceConfigKey = "TicketingService";

        public TicketingModule(
            IConfiguration configuration,
            IServiceCollection services)
        {
            _configuration = configuration;

            var baseUrl = _configuration.GetSection(TicketingServiceConfigKey)["InternalBaseUrl"];
            services.AddHttpProxyTicketing(baseUrl);
        }

        protected override void Load(ContainerBuilder builder)
        {
            var baseUrl = _configuration.GetSection(TicketingServiceConfigKey)["PublicBaseUrl"];
            builder
                .Register(c => new TicketingUrl(baseUrl))
                .As<ITicketingUrl>()
                .SingleInstance();
        }
    }
}
