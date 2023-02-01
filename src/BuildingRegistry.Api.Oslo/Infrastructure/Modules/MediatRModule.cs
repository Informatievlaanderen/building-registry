namespace BuildingRegistry.Api.Oslo.Infrastructure.Modules
{
    using Autofac;
    using Building.HandlersV2;
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
                builder.RegisterType<CountHandler>().AsImplementedInterfaces();
                builder.RegisterType<GetHandler>().AsImplementedInterfaces();
                builder.RegisterType<ListHandler>().AsImplementedInterfaces();

                builder.RegisterType<BuildingUnit.HandlersV2.CountHandler>().AsImplementedInterfaces();
                builder.RegisterType<BuildingUnit.HandlersV2.GetHandler>().AsImplementedInterfaces();
                builder.RegisterType<BuildingUnit.HandlersV2.ListHandler>().AsImplementedInterfaces();
            }
            else
            {
                builder.RegisterType<Building.Handlers.CountHandler>().AsImplementedInterfaces();
                builder.RegisterType<Building.Handlers.GetHandler>().AsImplementedInterfaces();
                builder.RegisterType<Building.Handlers.ListHandler>().AsImplementedInterfaces();

                builder.RegisterType<BuildingUnit.Handlers.CountHandler>().AsImplementedInterfaces();
                builder.RegisterType<BuildingUnit.Handlers.GetHandler>().AsImplementedInterfaces();
                builder.RegisterType<BuildingUnit.Handlers.ListHandler>().AsImplementedInterfaces();
            }
        }
    }
}
