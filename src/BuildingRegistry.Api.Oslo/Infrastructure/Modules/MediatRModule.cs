namespace BuildingRegistry.Api.Oslo.Infrastructure.Modules
{
    using Autofac;
    using Building.Count;
    using Building.Detail;
    using Building.List;
    using BuildingUnit.Count;
    using BuildingUnit.Detail;
    using BuildingUnit.List;
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

            if (_useProjectionsV2)
            {
                builder.RegisterType<BuildingCountHandlerV2>().AsImplementedInterfaces();
                builder.RegisterType<BuildingDetailHandlerV2>().AsImplementedInterfaces();
                builder.RegisterType<BuildingListHandlerV2>().AsImplementedInterfaces();

                builder.RegisterType<BuildingUnitCountHandlerV2>().AsImplementedInterfaces();
                builder.RegisterType<BuildingUnitDetailHandlerV2>().AsImplementedInterfaces();
                builder.RegisterType<BuildingUnitListHandlerV2>().AsImplementedInterfaces();
            }
            else
            {
                builder.RegisterType<BuildingCountHandler>().AsImplementedInterfaces();
                builder.RegisterType<BuildingDetailHandler>().AsImplementedInterfaces();
                builder.RegisterType<BuildingListHandler>().AsImplementedInterfaces();

                builder.RegisterType<BuildingUnitCountHandler>().AsImplementedInterfaces();
                builder.RegisterType<GetHandler>().AsImplementedInterfaces();
                builder.RegisterType<ListHandler>().AsImplementedInterfaces();
            }
        }
    }
}
