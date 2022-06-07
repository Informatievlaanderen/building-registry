namespace BuildingRegistry.Tests.ProjectionTests.Wms
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
    using NetTopologySuite.Geometries;
    using Projections.Wms.BuildingUnitV2;
    using Tests.Legacy.Autofixture;
    using Xunit;
    using Envelope = Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope;

    public class BuildingUnitV2Tests : BuildingWmsProjectionTest<BuildingUnitV2Projections>
    {
        private readonly Fixture? _fixture = new Fixture();
        
        public BuildingUnitV2Tests()
        {
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithBuildingStatus());
            _fixture.Customize(new WithBuildingGeometryMethod());
            _fixture.Customize(new WithValidExtendedWkbPoint());
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
        public async Task WhenNonRemovedBuildingWasMigrated(string buildingStatus, string? expectedStatus)
        {
            _fixture.Register(() => false);
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

                    var buildingUnits = ct.BuildingUnitsV2
                        .Where(unit => unit.BuildingPersistentLocalId == buildingWasMigrated.BuildingPersistentLocalId)
                        .ToList();

                    foreach (var unit in buildingWasMigrated.BuildingUnits)
                    {
                        var expectedUnit = buildingUnits
                            .Single(x => x.BuildingUnitPersistentLocalId == unit.BuildingUnitPersistentLocalId);

                        expectedUnit.BuildingPersistentLocalId.Should().Be(buildingWasMigrated.BuildingPersistentLocalId);
                        expectedUnit.Status.Should().Be(BuildingUnitStatus.Parse(unit.Status));
                        expectedUnit.Function.Should().Be(BuildingUnitV2Projections.MapFunction(BuildingUnitFunction.Parse(unit.Function)));
                        expectedUnit.PositionMethod.Should().Be(BuildingUnitV2Projections.MapGeometryMethod(BuildingUnitPositionGeometryMethod.Parse(unit.GeometryMethod)));
                        expectedUnit.Version.Should().Be(buildingWasMigrated.Provenance.Timestamp);
                        expectedUnit.Position.Should().BeEquivalentTo(unit.ExtendedWkbGeometry.ToByteArray());
                    }
                });
        }

        [Fact]
        public async Task WhenRemovedBuildingWasMigrated()
        {
            _fixture.Register(() => true);

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

                    var buildingUnits = ct.BuildingUnitsV2
                        .Where(unit => unit.BuildingPersistentLocalId == buildingWasMigrated.BuildingPersistentLocalId)
                        .ToList();

                    buildingUnits.Should().BeEmpty();
                });
        }

        [Fact]
        public async Task WhenNonRemovedBuilding_WithRemovedBuildingUnitWasMigrated()
        {
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

                    var buildingUnits = ct.BuildingUnitsV2
                        .Where(unit => unit.BuildingPersistentLocalId == buildingWasMigrated.BuildingPersistentLocalId)
                        .ToList();

                    var countNonRemoved = buildingWasMigrated.BuildingUnits.Count(x => !x.IsRemoved);

                    buildingUnits.Count.Should().Be(countNonRemoved);

                    foreach (var unit in buildingWasMigrated.BuildingUnits)
                    {
                        var expectedUnit = buildingUnits
                            .SingleOrDefault(x => x.BuildingUnitPersistentLocalId == unit.BuildingUnitPersistentLocalId);

                        if (unit.IsRemoved)
                        {
                            expectedUnit.Should().BeNull();
                        }
                        else
                        {
                            expectedUnit.BuildingPersistentLocalId.Should().Be(buildingWasMigrated.BuildingPersistentLocalId);
                            expectedUnit.Status.Should().Be(BuildingUnitStatus.Parse(unit.Status));
                            expectedUnit.Function.Should().Be(BuildingUnitV2Projections.MapFunction(BuildingUnitFunction.Parse(unit.Function)));
                            expectedUnit.PositionMethod.Should().Be(BuildingUnitV2Projections.MapGeometryMethod(BuildingUnitPositionGeometryMethod.Parse(unit.GeometryMethod)));
                            expectedUnit.Version.Should().Be(buildingWasMigrated.Provenance.Timestamp);
                            expectedUnit.Position.Should().BeEquivalentTo(unit.ExtendedWkbGeometry.ToByteArray());
                        }
                    }
                });
        }

        protected override BuildingUnitV2Projections CreateProjection() => new BuildingUnitV2Projections(WKBReaderFactory.Create());
    }
}
