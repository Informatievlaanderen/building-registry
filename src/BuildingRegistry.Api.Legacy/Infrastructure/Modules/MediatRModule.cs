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
        public MediatRModule()
        { }

        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<Mediator>()
                .As<IMediator>()
                .InstancePerLifetimeScope();

            builder.RegisterType<BuildingCountHandlerV2>().AsImplementedInterfaces();
            builder.RegisterType<GetDetailHandlerV2>().AsImplementedInterfaces();
            builder.RegisterType<ListHandlerV2>().AsImplementedInterfaces();

            builder.RegisterType<BuildingUnit.Count.CountHandlerV2>().AsImplementedInterfaces();
            builder.RegisterType<GetBuildingUnitDetailHandlerV2>().AsImplementedInterfaces();
            builder.RegisterType<BuildingUnit.List.BuildingUnitListHandlerV2>().AsImplementedInterfaces();

            builder.RegisterType<CrabGebouwenHandler>().AsImplementedInterfaces();
            builder.RegisterType<BuildingSyncHandler>().AsImplementedInterfaces();
            builder.RegisterType<BuildingDetailReferencesHandler>().AsImplementedInterfaces();
        }
    }
}
