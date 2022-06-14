namespace BuildingRegistry.Api.Extract.Infrastructure.Modules
{
    using Autofac;
    using Handlers;
    using MediatR;
    using Microsoft.Extensions.Configuration;
    using Module = Autofac.Module;

    public class MediatRModule : Module
    {
        private readonly IConfiguration _configuration;

        public MediatRModule(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<Mediator>()
                .As<IMediator>()
                .InstancePerLifetimeScope();

            // request & notification handlers
            builder.Register<ServiceFactory>(context =>
            {
                var ctx = context.Resolve<IComponentContext>();
                return type => ctx.Resolve(type);
            });

            var useProjectionsV2ConfigValue = _configuration.GetSection("FeatureToggles")["UseProjectionsV2"];
            var useProjectionsV2 = false;

            if (!string.IsNullOrEmpty(useProjectionsV2ConfigValue))
            {
                useProjectionsV2 = bool.Parse(useProjectionsV2ConfigValue);
            }

            if (useProjectionsV2)
            {
                builder.RegisterType<GetBuildingsHandlerV2>().AsImplementedInterfaces();
            }
            else
            {
                builder.RegisterType<GetBuildingsHandler>().AsImplementedInterfaces();
            }
        }
    }
}
