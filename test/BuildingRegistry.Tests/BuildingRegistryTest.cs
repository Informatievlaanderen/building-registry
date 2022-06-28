namespace BuildingRegistry.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Metrics;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Fixtures;
    using Microsoft.Extensions.Configuration;

    public class BuildingRegistryTest : AutofacBasedTest
    {
        protected Fixture Fixture { get; }
        protected Newtonsoft.Json.JsonSerializerSettings EventSerializerSettings { get; } = Be.Vlaanderen.Basisregisters.EventHandling.EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

        public void DispatchArrangeCommand<T>(T command) where T : IHasCommandProvenance
        {
            using var scope = Container.BeginLifetimeScope();
            var bus = scope.Resolve<ICommandHandlerResolver>();
            bus.Dispatch(command.CreateCommandId(), command);
        }

        public BuildingRegistryTest(Xunit.Abstractions.ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture = new Fixture();
            Fixture.Register(() => (Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting.ISnapshotStrategy)Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting.IntervalStrategy.Default);

            Fixture.Customize(new WithValidExtendedWkbPolygon());
            Fixture.Customize(new WithBuildingStatus());
            Fixture.Customize(new WithBuildingUnitPositionGeometryMethod());
            Fixture.Customize(new WithBuildingUnitFunction());
            Fixture.Customize(new WithBuildingGeometryMethod());
            Fixture.Customize(new WithBuildingUnitStatus());
            Fixture.Customize(new SetProvenanceImplementationsCallSetProvenance());

            Fixture.Register(() =>
            {
                var functions = new List<BuildingRegistry.Legacy.BuildingUnitFunction>
                {
                    BuildingRegistry.Legacy.BuildingUnitFunction.Common,
                    BuildingRegistry.Legacy.BuildingUnitFunction.Unknown,
                };

                return functions[new Random(Fixture.Create<int>()).Next(0, functions.Count - 1)];
            });

            Fixture.Register(() =>
            {
                var statusses = new List<BuildingRegistry.Legacy.BuildingUnitStatus>
                {
                    BuildingRegistry.Legacy.BuildingUnitStatus.NotRealized,
                    BuildingRegistry.Legacy.BuildingUnitStatus.Retired,
                    BuildingRegistry.Legacy.BuildingUnitStatus.Realized,
                    BuildingRegistry.Legacy.BuildingUnitStatus.Planned,
                };

                return statusses[new Random(Fixture.Create<int>()).Next(0, statusses.Count - 1)];
            });
            
            Fixture.Register(() =>
            {
                var method = new List<BuildingRegistry.Legacy.BuildingUnitPositionGeometryMethod>
                {
                    BuildingRegistry.Legacy.BuildingUnitPositionGeometryMethod.AppointedByAdministrator,
                    BuildingRegistry.Legacy.BuildingUnitPositionGeometryMethod.DerivedFromObject,
                };

                return method[new Random(Fixture.Create<int>()).Next(0, method.Count - 1)];
            });
        }

        protected override void ConfigureCommandHandling(Autofac.ContainerBuilder builder)
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string> { { "ConnectionStrings:Events", "x" } })
                .Build();

            builder
                .RegisterModule(new Infrastructure.Modules.CommandHandlingModule(configuration))
                .RegisterModule(new Be.Vlaanderen.Basisregisters.AggregateSource.SqlStreamStore.Autofac.SqlStreamStoreModule());
        }

        protected override void ConfigureEventHandling(ContainerBuilder builder)
        {
            var eventSerializerSettings = Be.Vlaanderen.Basisregisters.EventHandling.EventsJsonSerializerSettingsProvider.CreateSerializerSettings();
            builder.RegisterModule(new Be.Vlaanderen.Basisregisters.EventHandling.Autofac.EventHandlingModule(typeof(DomainAssemblyMarker).Assembly, eventSerializerSettings));
        }
        public string GetSnapshotIdentifier(string identifier) => $"{identifier}-snapshots";
    }
}
