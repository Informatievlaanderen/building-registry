namespace BuildingRegistry.Api.Legacy.Infrastructure.Modules
{
    using Autofac;
    using Microsoft.Extensions.Configuration;
    using ParcelMatching;
    using ParcelMatching.Wfs;

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
                .RegisterType<WfsParcelMatching>()
                .AsImplementedInterfaces();
        }
    }
}
