namespace BuildingRegistry.Infrastructure.Modules
{
    using Autofac;
    using Repositories;

    public class RepositoriesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // We could just scan the assembly for classes using Repository<> and registering them against the only interface they implement
            builder
                .RegisterType<LegacyBuildings>()
                .As<Legacy.IBuildings>();

            builder
                .RegisterType<Buildings>()
                .As<Building.IBuildings>();
        }
    }
}
