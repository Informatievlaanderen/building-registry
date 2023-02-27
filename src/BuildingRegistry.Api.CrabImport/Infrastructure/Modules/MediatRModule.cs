namespace BuildingRegistry.Api.CrabImport.Infrastructure.Modules
{
    using System.Reflection;
    using Autofac;
    using Handlers;
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

            builder.RegisterAssemblyTypes(typeof(PostHandler).GetTypeInfo().Assembly).AsImplementedInterfaces();
        }
    }
}
