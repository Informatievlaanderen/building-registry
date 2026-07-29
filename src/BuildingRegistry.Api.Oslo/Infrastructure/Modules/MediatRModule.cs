namespace BuildingRegistry.Api.Oslo.Infrastructure.Modules
{
    using Autofac;
    using MediatR;
    using Module = Autofac.Module;
    using V2 = Building.V2;
    using V2Unit = BuildingUnit.V2;
    using V3 = Building.V3;
    using V3Unit = BuildingUnit.V3;

    public class MediatRModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<Mediator>()
                .As<IMediator>()
                .InstancePerLifetimeScope();

            builder.RegisterType<V2.Count.BuildingCountHandlerV2>().AsImplementedInterfaces();
            builder.RegisterType<V2.Detail.BuildingDetailHandlerV2>().AsImplementedInterfaces();
            builder.RegisterType<V2.List.BuildingListHandlerV2>().AsImplementedInterfaces();
            builder.RegisterType<V2.Sync.BuildingSyncHandler>().AsImplementedInterfaces();

            builder.RegisterType<V2Unit.Count.BuildingUnitCountHandlerV2>().AsImplementedInterfaces();
            builder.RegisterType<V2Unit.Detail.BuildingUnitDetailHandlerV2>().AsImplementedInterfaces();
            builder.RegisterType<V2Unit.List.BuildingUnitListHandlerV2>().AsImplementedInterfaces();

            builder.RegisterType<V3.Count.BuildingCountHandler>().AsImplementedInterfaces();
            builder.RegisterType<V3.Detail.BuildingDetailHandler>().AsImplementedInterfaces();
            builder.RegisterType<V3.List.BuildingListHandler>().AsImplementedInterfaces();

            builder.RegisterType<V3Unit.Count.BuildingUnitCountHandler>().AsImplementedInterfaces();
            builder.RegisterType<V3Unit.Detail.BuildingUnitDetailHandler>().AsImplementedInterfaces();
            builder.RegisterType<V3Unit.List.BuildingUnitListHandler>().AsImplementedInterfaces();
        }
    }
}
