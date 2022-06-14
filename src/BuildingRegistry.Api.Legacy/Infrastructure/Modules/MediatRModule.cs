namespace BuildingRegistry.Api.Legacy.Infrastructure.Modules
{
    using Autofac;
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
                builder.RegisterType<Handlers.BuildingV2.CountHandler>().AsImplementedInterfaces();
                builder.RegisterType<Handlers.BuildingV2.GetHandler>().AsImplementedInterfaces();
                builder.RegisterType<Handlers.BuildingV2.ListHandler>().AsImplementedInterfaces();

                builder.RegisterType<Handlers.BuildingUnitV2.CountHandler>().AsImplementedInterfaces();
                builder.RegisterType<Handlers.BuildingUnitV2.GetHandler>().AsImplementedInterfaces();
                builder.RegisterType<Handlers.BuildingUnitV2.ListHandler>().AsImplementedInterfaces();
            }
            else
            {
                builder.RegisterType<Handlers.Building.CountHandler>().AsImplementedInterfaces();
                builder.RegisterType<Handlers.Building.GetHandler>().AsImplementedInterfaces();
                builder.RegisterType<Handlers.Building.ListHandler>().AsImplementedInterfaces();

                builder.RegisterType<Handlers.BuildingUnit.CountHandler>().AsImplementedInterfaces();
                builder.RegisterType<Handlers.BuildingUnit.GetHandler>().AsImplementedInterfaces();
                builder.RegisterType<Handlers.BuildingUnit.ListHandler>().AsImplementedInterfaces();
            }

            builder.RegisterType<Handlers.Building.CrabGebouwenHandler>().AsImplementedInterfaces();
            builder.RegisterType<Handlers.Building.SyncHandler>().AsImplementedInterfaces();
            builder.RegisterType<Handlers.Building.GetReferencesHandler>().AsImplementedInterfaces();
        }
    }
}
