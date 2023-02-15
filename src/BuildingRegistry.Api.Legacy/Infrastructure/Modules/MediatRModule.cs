namespace BuildingRegistry.Api.Legacy.Infrastructure.Modules
{
    using Autofac;
    using Building.Count;
    using Building.Crab;
    using Building.Detail;
    using Building.List;
    using Building.Sync;
    using BuildingUnit.Detail;
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
                builder.RegisterType<BuildingCountHandlerV2>().AsImplementedInterfaces();
                builder.RegisterType<GetDetailHandlerV2>().AsImplementedInterfaces();
                builder.RegisterType<ListHandlerV2>().AsImplementedInterfaces();

                builder.RegisterType<BuildingUnit.Count.CountHandlerV2>().AsImplementedInterfaces();
                builder.RegisterType<GetBuildingUnitDetailHandlerV2>().AsImplementedInterfaces();
                builder.RegisterType<BuildingUnit.List.BuildingUnitListHandlerV2>().AsImplementedInterfaces();
            }
            else
            {
                builder.RegisterType<BuildingCountHandler>().AsImplementedInterfaces();
                builder.RegisterType<GetDetailHandler>().AsImplementedInterfaces();
                builder.RegisterType<ListHandler>().AsImplementedInterfaces();

                builder.RegisterType<BuildingUnit.Count.BuildingUnitCountHandler>().AsImplementedInterfaces();
                builder.RegisterType<GetBuildingUnitDetailHandler>().AsImplementedInterfaces();
                builder.RegisterType<BuildingUnit.List.BuildingUnitListHandler>().AsImplementedInterfaces();
            }

            builder.RegisterType<CrabGebouwenHandler>().AsImplementedInterfaces();
            builder.RegisterType<BuildingSyncHandler>().AsImplementedInterfaces();
            builder.RegisterType<BuildingDetailReferencesHandler>().AsImplementedInterfaces();
        }
    }
}
