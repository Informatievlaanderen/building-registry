namespace BuildingRegistry.Infrastructure.Modules
{
    using Autofac;
    using Repositories;

    public class RepositoriesModule : Module
    {
        protected override void Load(ContainerBuilder containerBuilder)
        {
            // We could just scan the assembly for classes using Repository<> and registering them against the only interface they implement
            containerBuilder
                .RegisterType<LegacyBuildings>()
                .As<Legacy.IBuildings>();

            containerBuilder
                .RegisterType<Buildings>()
                .As<Building.IBuildings>();
        }
    }
}
