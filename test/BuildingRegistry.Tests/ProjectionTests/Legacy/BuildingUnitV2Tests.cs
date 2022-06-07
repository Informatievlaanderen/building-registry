namespace BuildingRegistry.Tests.ProjectionTests.Legacy
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Autofixture;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Building;
    using Building.Events;
    using FluentAssertions;
    using Projections.Legacy.BuildingUnitDetailV2;
    using Tests.Legacy.Autofixture;
    using Xunit;

    public class BuildingUnitV2Tests : BuildingLegacyProjectionTest<BuildingUnitDetailV2Projections>
    {
        private readonly Fixture? _fixture = new Fixture();
        
        public BuildingUnitV2Tests()
        {
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithBuildingStatus());
            _fixture.Customize(new WithBuildingGeometryMethod());
            _fixture.Customize(new WithValidExtendedWkbPolygon());
            _fixture.Customize(new WithBuildingUnitStatus());
            _fixture.Customize(new WithBuildingUnitFunction());
            _fixture.Customize(new WithBuildingUnitPositionGeometryMethod());
        }

        [Theory]
        [InlineData("Planned", null)]
        [InlineData("UnderConstruction", null)]
        [InlineData("Realized", null)]
        [InlineData("Retired", "Retired")]
        [InlineData("NotRealized", "NotRealized")]
        public async Task WhenBuildingWasMigrated(string buildingStatus, string? expectedStatus)
        {
            _fixture.Register(() => BuildingStatus.Parse(buildingStatus));

            var buildingWasMigrated = _fixture.Create<BuildingWasMigrated>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasMigrated.GetHash() }
            };

            await Sut
                .Given(new Envelope<BuildingWasMigrated>(new Envelope(buildingWasMigrated, metadata)))
                .Then(async ct =>
                {
                    var buildingUnitBuildingItem = (await ct.BuildingUnitBuildingsV2.FindAsync(buildingWasMigrated.BuildingPersistentLocalId));
                    buildingUnitBuildingItem.Should().NotBeNull();

                    buildingUnitBuildingItem.IsRemoved.Should().Be(buildingWasMigrated.IsRemoved);
                    if (string.IsNullOrEmpty(expectedStatus))
                    {
                        buildingUnitBuildingItem.BuildingRetiredStatus.Should().BeNull();
                    }
                    else
                    {
                        buildingUnitBuildingItem.BuildingRetiredStatus.Value.Value.Should().Be(expectedStatus);
                    }

                    var buildingUnits = ct.BuildingUnitDetailsV2
                        .Where(unit => unit.BuildingPersistentLocalId == buildingWasMigrated.BuildingPersistentLocalId)
                        .ToList();

                    foreach (var unit in buildingWasMigrated.BuildingUnits)
                    {
                        var expectedUnit = buildingUnits
                            .Single(x => x.BuildingUnitPersistentLocalId == unit.BuildingUnitPersistentLocalId);

                        expectedUnit.BuildingPersistentLocalId.Should().Be(buildingWasMigrated.BuildingPersistentLocalId);
                        expectedUnit.IsRemoved.Should().Be(unit.IsRemoved);
                        expectedUnit.Status.Status.Should().Be(unit.Status);
                        expectedUnit.Function.Function.Should().Be(unit.Function);
                        expectedUnit.PositionMethod.GeometryMethod.Should().Be(unit.GeometryMethod);
                        expectedUnit.Version.Should().Be(buildingWasMigrated.Provenance.Timestamp);
                        expectedUnit.Position.Should().BeEquivalentTo(unit.ExtendedWkbGeometry.ToByteArray());
                        expectedUnit.Addresses.Should().NotBeEmpty();
                        expectedUnit.Addresses.Should().BeEquivalentTo(unit.AddressPersistentLocalIds.Select(x => new BuildingUnitDetailAddressItemV2
                        {
                            BuildingUnitPersistentLocalId = unit.BuildingUnitPersistentLocalId,
                            AddressPersistentLocalId = x,
                            Count = 1
                        }));
                    }
                });
        }

        protected override BuildingUnitDetailV2Projections CreateProjection() => new BuildingUnitDetailV2Projections();
    }
}
