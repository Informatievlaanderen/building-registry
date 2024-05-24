namespace BuildingRegistry.Api.Legacy.Infrastructure.Modules
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
            builder.RegisterType<GetDetailHandlerV2>().AsImplementedInterfaces();
            builder.RegisterType<ListHandlerV2>().AsImplementedInterfaces();

            builder.RegisterType<CountHandlerV2>().AsImplementedInterfaces();
            builder.RegisterType<GetBuildingUnitDetailHandlerV2>().AsImplementedInterfaces();
            builder.RegisterType<BuildingUnitListHandlerV2>().AsImplementedInterfaces();

            builder.RegisterType<BuildingSyncHandler>().AsImplementedInterfaces();
        }
    }
}
