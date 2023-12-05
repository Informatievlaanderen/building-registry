namespace BuildingRegistry.Api.Extract.Infrastructure.Modules
{
    using Autofac;
    using Extracts;
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

            builder.RegisterType<GetBuildingsHandlerV2>().AsImplementedInterfaces();
            builder.RegisterType<GetBuildingUnitAddressLinksHandler>().AsImplementedInterfaces();
        }
    }
}
