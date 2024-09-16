namespace BuildingRegistry.Api.Oslo.Infrastructure.Modules
{
    using Autofac;
    using Consumer.Read.Parcel;
    using Projections.Legacy;

    public class ParcelBuildingMatchingModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<ParcelMatching>()
                .AsImplementedInterfaces();

            builder
                .RegisterType<BuildingMatching>()
                .AsImplementedInterfaces();
        }
    }
}
