namespace BuildingRegistry.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Autofac;
    using AutoFixture;
    using BackOffice;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.SqlStreamStore.Autofac;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building;
    using Fixtures;
    using Infrastructure.Modules;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using Newtonsoft.Json;
    using Xunit.Abstractions;
    using BuildingUnitFunction = BuildingRegistry.Legacy.BuildingUnitFunction;
    using BuildingUnitPositionGeometryMethod = BuildingRegistry.Legacy.BuildingUnitPositionGeometryMethod;
    using BuildingUnitStatus = BuildingRegistry.Legacy.BuildingUnitStatus;

    public class BuildingRegistryTest : AutofacBasedTest
    {
        protected FakeConsumerAddressContext FakeConsumerAddressContext;
        protected Mock<IBuildingGeometries> FakeBuildingGeometries = new Mock<IBuildingGeometries>();

        protected Fixture Fixture { get; }
        protected string ConfigDetailUrl => "http://base/{0}";
        protected JsonSerializerSettings EventSerializerSettings { get; } = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

        public void DispatchArrangeCommand<T>(T command) where T : IHasCommandProvenance
        {
            using var scope = Container.BeginLifetimeScope();
            var bus = scope.Resolve<ICommandHandlerResolver>();
            bus.Dispatch(command.CreateCommandId(), command).GetAwaiter().GetResult();
        }

        public BuildingRegistryTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture = new Fixture();

            Fixture.Customize(new WithValidExtendedWkbPolygon());
            Fixture.Customize(new WithBuildingStatus());
            Fixture.Customize(new WithBuildingUnitPositionGeometryMethod());
            Fixture.Customize(new WithBuildingUnitFunction());
            Fixture.Customize(new WithBuildingGeometryMethod());
            Fixture.Customize(new WithBuildingUnitStatus());
            Fixture.Customize(new SetProvenanceImplementationsCallSetProvenance());
            Fixture.Customize(new RandomBooleanSequenceCustomization());
            Fixture.Customizations.Add(new RandomUniqueIntegerGenerator());
            Fixture.Register(() => (ISnapshotStrategy)NoSnapshotStrategy.Instance);

            Fixture.Register(() =>
            {
                var functions = new List<BuildingUnitFunction>
                {
                    BuildingUnitFunction.Unknown,
                };

                return functions[new Random(Fixture.Create<int>()).Next(0, functions.Count - 1)];
            });

            Fixture.Register(() =>
            {
                var statusses = new List<BuildingUnitStatus>
                {
                    BuildingUnitStatus.NotRealized,
                    BuildingUnitStatus.Retired,
                    BuildingUnitStatus.Realized,
                    BuildingUnitStatus.Planned,
                };

                return statusses[new Random(Fixture.Create<int>()).Next(0, statusses.Count - 1)];
            });

            Fixture.Register(() =>
            {
                var method = new List<BuildingUnitPositionGeometryMethod>
                {
                    BuildingUnitPositionGeometryMethod.AppointedByAdministrator,
                    BuildingUnitPositionGeometryMethod.DerivedFromObject,
                };

                return method[new Random(Fixture.Create<int>()).Next(0, method.Count - 1)];
            });
        }

        protected override void ConfigureCommandHandling(ContainerBuilder builder)
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string> { { "ConnectionStrings:Events", "x" } })
                .AddInMemoryCollection(new Dictionary<string, string> { { "ConnectionStrings:Snapshots", "x" } })
                .AddInMemoryCollection(new Dictionary<string, string> { { "BuildingDetailUrl", ConfigDetailUrl } })
                .AddInMemoryCollection(new Dictionary<string, string> { { "BuildingUnitDetailUrl", ConfigDetailUrl } })
                .AddInMemoryCollection(new Dictionary<string, string> { { "AnoApiToggle", "true" } })
                .AddInMemoryCollection(new Dictionary<string, string> { { "AutomaticBuildingUnitCreationToggle", "true" } })
                .Build();

            builder.Register(a => (IConfiguration)configuration);

            builder
                .RegisterModule(new CommandHandlingModule(configuration))
                .RegisterModule(new SqlStreamStoreModule());

            builder.RegisterModule(new SqlSnapshotStoreModule());

            FakeConsumerAddressContext = new FakeConsumerAddressContextFactory().CreateDbContext();

            builder
                .Register(_ => FakeConsumerAddressContext)
                .SingleInstance()
                .As<IAddresses>()
                .AsSelf();

            if (!FakeBuildingGeometries.Setups.Any())
            {
                FakeBuildingGeometries
                    .Setup(x => x.GetOverlappingBuildings(
                        It.IsAny<BuildingPersistentLocalId>(),
                        It.IsAny<ExtendedWkbGeometry>()))
                    .Returns(new List<BuildingGeometryData>());

                FakeBuildingGeometries
                    .Setup(x => x.GetOverlappingBuildingOutlines(
                        It.IsAny<BuildingPersistentLocalId>(),
                        It.IsAny<ExtendedWkbGeometry>()))
                    .Returns(new List<BuildingGeometryData>());
            }

            builder
                .Register(_ => FakeBuildingGeometries.Object)
                .SingleInstance()
                .As<IBuildingGeometries>()
                .AsSelf();

            builder
                .Register(c => new BuildingFactory(Fixture.Create<ISnapshotStrategy>()))
                .As<IBuildingFactory>();
        }

        protected override void ConfigureEventHandling(ContainerBuilder builder)
        {
            var eventSerializerSettings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();
            builder.RegisterModule(new EventHandlingModule(typeof(DomainAssemblyMarker).Assembly, eventSerializerSettings));
        }
        public string GetSnapshotIdentifier(string identifier) => $"{identifier}-snapshots";
    }
}
