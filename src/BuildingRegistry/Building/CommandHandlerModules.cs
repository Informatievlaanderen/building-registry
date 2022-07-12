namespace BuildingRegistry.Building
{
    using Autofac;
    using Be.Vlaanderen.Basisregisters.CommandHandling;

    public static class CommandHandlerModules
    {
        public static void Register(ContainerBuilder containerBuilder)
        {
            containerBuilder
                .RegisterType<ProvenanceFactory<Building>>()
                .AsSelf()
                .AsImplementedInterfaces()
                .SingleInstance();

            containerBuilder
                .RegisterType<BuildingCommandHandlerModule>()
                .Named<CommandHandlerModule>(typeof(BuildingCommandHandlerModule).FullName)
                .As<CommandHandlerModule>();

            containerBuilder
                .RegisterType<BuildingUnitCommandHandlerModule>()
                .Named<CommandHandlerModule>(typeof(BuildingUnitCommandHandlerModule).FullName)
                .As<CommandHandlerModule>();
        }
    }
}
