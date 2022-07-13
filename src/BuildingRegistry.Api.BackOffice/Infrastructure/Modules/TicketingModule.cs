namespace BuildingRegistry.Api.BackOffice.Infrastructure.Modules
{
    using Autofac;
    using Microsoft.Extensions.Configuration;
    using TicketingService.Abstractions;
    using TicketingService.Proxy.HttpProxy;

    public class TicketingModule : Module
    {
        private readonly IConfiguration _configuration;

        public TicketingModule(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        protected override void Load(ContainerBuilder builder)
        {
            var baseUrl = _configuration["TicketingService:BaseUrl"];
            builder
                .RegisterInstance(new HttpProxyTicketing(baseUrl))
                .As<ITicketing>()
                .SingleInstance();
        }
    }
}
