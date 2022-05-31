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
                .SingleInstance();

            containerBuilder
                .RegisterType<BuildingCommandHandlerModule>()
                .Named<CommandHandlerModule>(typeof(BuildingCommandHandlerModule).FullName)
                .As<CommandHandlerModule>();
        }
    }
}
