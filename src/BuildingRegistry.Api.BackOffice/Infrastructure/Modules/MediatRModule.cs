namespace BuildingRegistry.Api.BackOffice.Infrastructure.Modules
{
    using System.Linq;
    using System.Reflection;
    using Autofac;
    using Handlers.Building;
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

            builder
                .RegisterAssemblyTypes(typeof(PlanBuildingSqsHandler).GetTypeInfo().Assembly)
                .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)))
                .AsImplementedInterfaces();
        }
    }
}
