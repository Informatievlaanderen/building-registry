namespace BuildingRegistry.Tests
{
    using Autofac;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Xunit.Abstractions;

    public class BuildingRegistryHandlerTest : BuildingRegistryTest
    {
        public BuildingRegistryHandlerTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        public void DispatchArrangeCommand<T>(T command) where T : IHasCommandProvenance
        {
            using var scope = Container.BeginLifetimeScope();
            var bus = scope.Resolve<ICommandHandlerResolver>();
            bus.Dispatch(command.CreateCommandId(), command);
        }
    }
}
