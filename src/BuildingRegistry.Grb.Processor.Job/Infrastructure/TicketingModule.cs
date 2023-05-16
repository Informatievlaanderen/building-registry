namespace BuildingRegistry.Grb.Processor.Job.Infrastructure
{
    using Autofac;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using TicketingService.Abstractions;
    using TicketingService.Proxy.HttpProxy;

    public class TicketingModule : Module
    {
        private readonly string _baseUrl;

        public TicketingModule(
            IConfiguration configuration,
            IServiceCollection services)
        {
            _baseUrl = configuration["TicketingUrl"];
            services
                .AddHttpProxyTicketing(_baseUrl);
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder
                .Register(c => new TicketingUrl(_baseUrl))
                .As<ITicketingUrl>()
                .SingleInstance();
        }
    }
}
