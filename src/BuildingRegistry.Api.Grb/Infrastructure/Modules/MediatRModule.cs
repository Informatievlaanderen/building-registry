namespace BuildingRegistry.Api.Grb.Infrastructure.Modules
{
    using Autofac;
    using MediatR;
    using Uploads;

    public class MediatRModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<Mediator>()
                .As<IMediator>()
                .InstancePerLifetimeScope();

            builder.RegisterType<PreSignedUrlHandler>().AsImplementedInterfaces();
        }
    }
}
