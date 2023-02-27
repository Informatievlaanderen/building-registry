namespace BuildingRegistry.Api.Extract.Infrastructure.Modules
{
    using Autofac;
    using Extracts;
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
                builder.RegisterType<GetBuildingsHandlerV2>().AsImplementedInterfaces();
            }
            else
            {
                builder.RegisterType<GetBuildingsHandler>().AsImplementedInterfaces();
            }
        }
    }
}
