namespace BuildingRegistry.Tests
{
    using System.Collections.Generic;
    using Autofac;
    using Autofixture;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.SqlStreamStore.Autofac;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing.SqlStreamStore.Autofac;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
    using Building;
    using Building.Events;
    using Infrastructure.Modules;
    using Microsoft.Extensions.Configuration;
    using Xunit.Abstractions;

    public class SnapshotBasedTest : AutofacBasedTest
    {
        protected Fixture Fixture { get; }

        public SnapshotBasedTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture = new Fixture();
            Fixture
                .Customize(new WithSnapshotInterval(1000))
                .Customize(new SetProvenanceImplementationsCallSetProvenance())
                .Customize(new NodaTimeCustomization());

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string> { { "ConnectionStrings:Events", "x" } })
                .Build();

            var containerBuilder = new ContainerBuilder();

            containerBuilder
                .RegisterModule(new EventHandlingModule(typeof(DomainAssemblyMarker).Assembly, EventSerializerSettings))
                .RegisterModule(new CommandHandlingModule(configuration))
                .RegisterModule(new SqlStreamStoreModule());

            var eventMappingDictionary =
                new Dictionary<string, System.Type>(
                    EventMapping.DiscoverEventNamesInAssembly(typeof(DomainAssemblyMarker).Assembly))
                {
                    {$"{nameof(SnapshotContainer)}<{nameof(BuildingSnapshot)}>", typeof(SnapshotContainer)}
                };

            containerBuilder.RegisterInstance(new EventMapping(eventMappingDictionary)).As<EventMapping>();

            containerBuilder
                .Register(c => new BuildingFactory(Fixture.Create<ISnapshotStrategy>()))
                .As<IBuildingFactory>();

            containerBuilder.UseAggregateSourceTesting(CreateFactComparer(), CreateExceptionComparer());

            containerBuilder.RegisterInstance(testOutputHelper);
            containerBuilder.RegisterType<XUnitLogger>().AsImplementedInterfaces();
            containerBuilder.RegisterType<FakePersistentLocalIdGenerator>().As<IPersistentLocalIdGenerator>();

            Container = containerBuilder.Build();
        }
    }
}
