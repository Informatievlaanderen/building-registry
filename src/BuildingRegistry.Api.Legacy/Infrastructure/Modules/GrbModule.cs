namespace BuildingRegistry.Api.Legacy.Infrastructure.Modules
{
    using Abstractions.Infrastructure.Grb;
    using Autofac;
    using BuildingRegistry.Api.Legacy.Abstractions.Infrastructure.Grb.Wfs;
    using Microsoft.Extensions.Configuration;

    public class GrbModule : Module
    {
        private readonly IConfiguration _configuration;

        public GrbModule(IConfiguration configuration)
            => _configuration = configuration;

        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterInstance(new GrbWfsConfiguration(_configuration["Grb:Wfs:Url"]))
                .AsSelf();
                
            builder
                .RegisterType<GrbWfsClient>()
                .AsImplementedInterfaces();
                
            builder
                .RegisterType<GrbBuildingParcel>()
                .AsImplementedInterfaces();
        }
    }
}
