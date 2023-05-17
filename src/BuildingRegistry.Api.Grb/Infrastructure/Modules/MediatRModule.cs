namespace BuildingRegistry.Api.Grb.Infrastructure.Modules
{
    using System.Linq;
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

            builder
                .RegisterAssemblyTypes(typeof(UploadPreSignedUrlHandler).Assembly)
                .Where(t => t
                    .GetInterfaces()
                    .Any(i => i.IsGenericType
                              && (i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)
                                  || i.GetGenericTypeDefinition() == typeof(IRequestHandler<>))))
                .AsImplementedInterfaces();
        }
    }
}
