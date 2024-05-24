namespace BuildingRegistry.Api.Oslo.Infrastructure.Modules
{
    using Autofac;
    using ParcelMatching;

    public class ParcelMatchingModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<ParcelMatching>()
                .AsImplementedInterfaces();
        }
    }
}
