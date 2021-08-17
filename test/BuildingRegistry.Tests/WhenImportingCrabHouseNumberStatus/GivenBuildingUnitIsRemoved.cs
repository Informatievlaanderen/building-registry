namespace BuildingRegistry.Tests.WhenImportingCrabHouseNumberStatus
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Autofixture;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Building.Commands.Crab;
    using Building.DataStructures;
    using Building.Events;
    using Building.Events.Crab;
    using ValueObjects;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingUnitIsRemoved : SnapshotBasedTest
    {
        public GivenBuildingUnitIsRemoved(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithNoDeleteModification());
            Fixture.Customize(new WithFixedBuildingUnitIdFromHouseNumber());
        }

        [Fact]
        public void ThenOnlyLegacyEventIsApplied()
        {
            Fixture.Customize(new WithSnapshotInterval(1));
            Fixture.Customize(new WithNoDeleteModification());

            var command = Fixture.Create<ImportHouseNumberStatusFromCrab>();
            var buildingId = Fixture.Create<BuildingId>();
            var buildingUnitWasAdded = Fixture.Create<BuildingUnitWasAdded>();

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    buildingUnitWasAdded,
                    Fixture.Create<BuildingUnitWasRemoved>())
                .When(command)
                .Then(new Fact[]
                {
                    new Fact(buildingId, command.ToLegacyEvent()),
                    new Fact(GetSnapshotIdentifier(buildingId),
                        BuildingSnapshotBuilder.CreateDefaultSnapshot(buildingId)
                            .WithHouseNumberStatusEventsByHouseNumberId(new Dictionary<AddressId, List<AddressHouseNumberStatusWasImportedFromCrab>>
                            {
                                { AddressId.CreateFor(command.HouseNumberId), new List<AddressHouseNumberStatusWasImportedFromCrab>{ command.ToLegacyEvent() } }
                            })
                            .WithBuildingUnitCollection(BuildingUnitCollectionSnapshotBuilder.CreateDefaultSnapshot()
                                .WithBuildingUnits(new List<BuildingUnitSnapshot>
                                {
                                    BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(buildingUnitWasAdded)
                                        .WithRemoved()
                                }))
                            .Build(3, EventSerializerSettings))
                }));
        }
    }
}
