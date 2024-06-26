namespace BuildingRegistry.Api.Oslo.Infrastructure.Modules
{
    using Autofac;
    using Building.Count;
    using Building.Detail;
    using Building.List;
    using Building.Sync;
    using BuildingUnit.Count;
    using BuildingUnit.Detail;
    using BuildingUnit.List;
    using MediatR;
    using Module = Autofac.Module;

    public class MediatRModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<Mediator>()
                .As<IMediator>()
                .InstancePerLifetimeScope();

            builder.RegisterType<BuildingCountHandlerV2>().AsImplementedInterfaces();
            builder.RegisterType<BuildingDetailHandlerV2>().AsImplementedInterfaces();
            builder.RegisterType<BuildingListHandlerV2>().AsImplementedInterfaces();

            builder.RegisterType<BuildingUnitCountHandlerV2>().AsImplementedInterfaces();
            builder.RegisterType<BuildingUnitDetailHandlerV2>().AsImplementedInterfaces();
            builder.RegisterType<BuildingUnitListHandlerV2>().AsImplementedInterfaces();

            builder.RegisterType<BuildingSyncHandler>().AsImplementedInterfaces();
        }
    }
}
