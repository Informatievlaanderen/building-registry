namespace BuildingRegistry.Api.BackOffice.Infrastructure.Modules
{
    using System.Reflection;
    using Autofac;
    using MediatR;
    using Module = Autofac.Module;
    using PlanBuildingSqsHandler = Handlers.Building.PlanBuildingSqsHandler;

    public class MediatRModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<Mediator>()
                .As<IMediator>()
                .InstancePerLifetimeScope();

            builder.RegisterAssemblyTypes(typeof(PlanBuildingSqsHandler).GetTypeInfo().Assembly).AsImplementedInterfaces();
        }
    }
}
