namespace BuildingRegistry.Api.Oslo.Infrastructure.Modules
{
    using Autofac;
    using MediatR;
    using Module = Autofac.Module;

    public class MediatRModule : Module
    {
        private readonly bool _useProjectionsV2;

        public MediatRModule(bool useProjectionsV2)
        {
            _useProjectionsV2 = useProjectionsV2;
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
            
            if (_useProjectionsV2)
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
        }
    }
}
