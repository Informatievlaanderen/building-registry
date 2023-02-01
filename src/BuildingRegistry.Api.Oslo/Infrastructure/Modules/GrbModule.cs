namespace BuildingRegistry.Api.Oslo.Infrastructure.Modules
{
    using Autofac;
    using Grb;
    using Grb.Wfs;
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
