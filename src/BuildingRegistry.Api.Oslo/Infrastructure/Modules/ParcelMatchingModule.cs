namespace BuildingRegistry.Api.Oslo.Infrastructure.Modules
{
    using Autofac;
    using Microsoft.Extensions.Configuration;
    using ParcelMatching;
    using ParcelMatching.Wfs;

    public class ParcelMatchingModule : Module
    {
        private readonly IConfiguration _configuration;
        private readonly bool _useProjectionsV2;

        public ParcelMatchingModule(IConfiguration configuration, bool useProjectionsV2)
        {
            _configuration = configuration;
            _useProjectionsV2 = useProjectionsV2;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterInstance(new GrbWfsConfiguration(_configuration["Grb:Wfs:Url"]))
                .AsSelf();

            if (_useProjectionsV2)
            {
                builder
                    .RegisterType<ParcelMatching>()
                    .AsImplementedInterfaces();
            }
            else
            {
                builder
                    .RegisterType<WfsParcelMatching>()
                    .AsImplementedInterfaces();
                builder
                    .RegisterType<GrbWfsClient>()
                    .AsImplementedInterfaces();
            }
        }
    }
}
