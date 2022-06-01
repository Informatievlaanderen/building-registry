namespace BuildingRegistry.Api.Oslo.Infrastructure.Modules
{
    using Abstractions.Infrastructure.Grb;
    using Abstractions.Infrastructure.Grb.Wfs;
    using Autofac;
    using Microsoft.Extensions.Configuration;

    public class GrbModule : Module
    {
        private readonly IConfiguration _configuration;

        public GrbModule(IConfiguration configuration)
            => _configuration = configuration;

        protected override void Load(ContainerBuilder containerBuilder)
        {
            containerBuilder
                .RegisterInstance(new GrbWfsConfiguration(_configuration["Grb:Wfs:Url"]))
                .AsSelf();
                
            containerBuilder
                .RegisterType<GrbWfsClient>()
                .AsImplementedInterfaces();
                
            containerBuilder
                .RegisterType<GrbBuildingParcel>()
                .AsImplementedInterfaces();
        }
    }
}
