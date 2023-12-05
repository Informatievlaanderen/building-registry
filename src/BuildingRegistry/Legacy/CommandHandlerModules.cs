namespace BuildingRegistry.Legacy
{
    using System;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.CommandHandling;

    [Obsolete("This is a legacy class and should not be used anymore.")]
    public static class CommandHandlerModules
    {
        public static void Register(ContainerBuilder containerBuilder)
        {
            containerBuilder
                .RegisterType<BuildingProvenanceFactory>()
                .SingleInstance();

            containerBuilder
                .RegisterType<BuildingLegacyProvenanceFactory>()
                .SingleInstance();

            containerBuilder
                .RegisterType<FixGrar1359ProvenanceFactory>()
                .SingleInstance();

            containerBuilder
                .RegisterType<PersistentLocalIdentifierProvenanceFactory>()
                .SingleInstance();

            containerBuilder
                .RegisterType<ReaddressingProvenanceFactory>()
                .SingleInstance();

            containerBuilder
                .RegisterType<BuildingCommandHandlerModule>()
                .Named<CommandHandlerModule>(typeof(BuildingCommandHandlerModule).FullName)
                .As<CommandHandlerModule>();
        }
    }
}
