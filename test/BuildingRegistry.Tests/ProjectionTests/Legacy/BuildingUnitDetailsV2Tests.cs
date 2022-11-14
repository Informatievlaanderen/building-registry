namespace BuildingRegistry.Tests.ProjectionTests.Legacy
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Building;
    using Building.Events;
    using Fixtures;
    using FluentAssertions;
    using Projections.Legacy.BuildingUnitDetailV2;
    using Tests.Legacy.Autofixture;
    using Xunit;

    public class BuildingUnitDetailsV2Tests : BuildingLegacyProjectionTest<BuildingUnitDetailV2Projections>
    {
        private readonly Fixture _fixture = new Fixture();

        public BuildingUnitDetailsV2Tests()
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
                    var buildingUnitBuildingItem = await ct.BuildingUnitBuildingsV2.FindAsync(buildingWasMigrated.BuildingPersistentLocalId);
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

                        expectedUnit.LastEventHash.Should().Be(buildingWasMigrated.GetHash());
                    }
                });
        }

        [Fact]
        public async Task WhenBuildingWasPlanned()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() }
            };

            await Sut
                .Given(new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, metadata)))
                .Then(async ct =>
                {
                    var buildingDetailItemV2 = await ct.BuildingUnitBuildingsV2.FindAsync(buildingWasPlannedV2.BuildingPersistentLocalId);
                    buildingDetailItemV2.Should().NotBeNull();

                    buildingDetailItemV2.IsRemoved.Should().BeFalse();
                    buildingDetailItemV2.BuildingRetiredStatus.Should().BeNull();
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasPlannedV2()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() }
            };

            await Sut
                .Given(new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, metadata)))
                .Then(async ct =>
                {
                    var item = await ct.BuildingUnitDetailsV2.FindAsync(buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId);
                    item.Should().NotBeNull();
                    item.BuildingPersistentLocalId.Should().Be(buildingUnitWasPlannedV2.BuildingPersistentLocalId);
                    item.Position.Should().BeEquivalentTo(buildingUnitWasPlannedV2.ExtendedWkbGeometry.ToByteArray());
                    item.PositionMethod.Should().Be(BuildingUnitPositionGeometryMethod.Parse(buildingUnitWasPlannedV2.GeometryMethod));
                    item.Function.Should()
                        .Be(BuildingUnitFunction.Parse(buildingUnitWasPlannedV2.Function));
                    item.Version.Should().Be(buildingUnitWasPlannedV2.Provenance.Timestamp);
                    item.IsRemoved.Should().BeFalse();
                    item.Status.Should().Be(BuildingUnitStatus.Planned);
                });
        }

        [Fact]
        public async Task WhenBuildingOutlineWasChanged()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();

            var @event = _fixture.Create<BuildingOutlineWasChanged>();

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasPlannedV2>(
                        new Envelope(
                            buildingUnitWasPlannedV2,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() } })),
                    new Envelope<BuildingOutlineWasChanged>(
                        new Envelope(
                            @event,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, @event.GetHash() }
                            })))
                .Then(async ct =>
                {
                    var item = await ct.BuildingUnitDetailsV2.FindAsync(buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId);
                    item.Should().NotBeNull();

                    item!.PositionMethod.Should().Be(BuildingUnitPositionGeometryMethod.DerivedFromObject);
                    item.Position.Should().BeEquivalentTo(@event.ExtendedWkbGeometryBuildingUnits!.ToByteArray());
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasRealizedV2()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();

            var @event = _fixture.Create<BuildingUnitWasRealizedV2>();

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasPlannedV2>(
                        new Envelope(
                            buildingUnitWasPlannedV2,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() } })),
                    new Envelope<BuildingUnitWasRealizedV2>(
                        new Envelope(
                            @event,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, @event.GetHash() }
                })))
                .Then(async ct =>
                {
                    var item = await ct.BuildingUnitDetailsV2.FindAsync(@event.BuildingUnitPersistentLocalId);
                    item.Should().NotBeNull();

                    item!.Status.Should().Be(BuildingUnitStatus.Realized);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasRealizedBecauseBuildingWasRealized()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();

            var @event = _fixture.Create<BuildingUnitWasRealizedBecauseBuildingWasRealized>();

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasPlannedV2>(
                        new Envelope(
                            buildingUnitWasPlannedV2,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() } })),
                    new Envelope<BuildingUnitWasRealizedBecauseBuildingWasRealized>(
                        new Envelope(
                            @event,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, @event.GetHash() }
                })))
                .Then(async ct =>
                {
                    var item = await ct.BuildingUnitDetailsV2.FindAsync(@event.BuildingUnitPersistentLocalId);
                    item.Should().NotBeNull();

                    item!.Status.Should().Be(BuildingUnitStatus.Realized);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasCorrectedFromRealizedToPlanned()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitWasRealizedV2 = _fixture.Create<BuildingUnitWasRealizedV2>();

            var @event = _fixture.Create<BuildingUnitWasCorrectedFromRealizedToPlanned>();

            await Sut
                .Given(new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, new Dictionary<string, object>
                {
                    { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() }
                })),
                    new Envelope<BuildingUnitWasRealizedV2>(new Envelope(buildingUnitWasRealizedV2, new Dictionary<string, object>
                {
                    { AddEventHashPipe.HashMetadataKey, buildingUnitWasRealizedV2.GetHash() }
                })),
                    new Envelope<BuildingUnitWasCorrectedFromRealizedToPlanned>(new Envelope(@event, new Dictionary<string, object>
                {
                    { AddEventHashPipe.HashMetadataKey, @event.GetHash() }
                })))
                .Then(async ct =>
                {
                    var item = await ct.BuildingUnitDetailsV2.FindAsync(@event.BuildingUnitPersistentLocalId);
                    item.Should().NotBeNull();

                    item.IsRemoved.Should().BeFalse();
                    item.Status.Should().Be(BuildingUnitStatus.Planned);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasCorrectedFromRealizedToPlannedBecauseBuildingWasCorrected()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitWasRealizedV2 = _fixture.Create<BuildingUnitWasRealizedV2>();
            var @event = _fixture.Create<BuildingUnitWasCorrectedFromRealizedToPlannedBecauseBuildingWasCorrected>();

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasPlannedV2>(
                        new Envelope(
                            buildingUnitWasPlannedV2,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() } })),
                    new Envelope<BuildingUnitWasRealizedV2>(
                        new Envelope(
                            buildingUnitWasRealizedV2,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, buildingUnitWasRealizedV2.GetHash() } })),
                    new Envelope<BuildingUnitWasCorrectedFromRealizedToPlannedBecauseBuildingWasCorrected>(
                        new Envelope(
                            @event,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, @event.GetHash() } })))
                .Then(async ct =>
                {
                    var item = await ct.BuildingUnitDetailsV2.FindAsync(@event.BuildingUnitPersistentLocalId);
                    item.Should().NotBeNull();

                    item!.Status.Should().Be(BuildingUnitStatus.Planned);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasNotRealizedV2()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();

            var @event = _fixture.Create<BuildingUnitWasNotRealizedV2>();

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasPlannedV2>(
                        new Envelope(
                            buildingUnitWasPlannedV2,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() } })),
                    new Envelope<BuildingUnitWasNotRealizedV2>(
                        new Envelope(
                            @event,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, @event.GetHash() } })))
                .Then(async ct =>
                {
                    var item = await ct.BuildingUnitDetailsV2.FindAsync(@event.BuildingUnitPersistentLocalId);
                    item.Should().NotBeNull();

                    item!.Status.Should().Be(BuildingUnitStatus.NotRealized);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasNotRealizedBecauseBuildingWasNotRealized()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();

            var @event = _fixture.Create<BuildingUnitWasNotRealizedBecauseBuildingWasNotRealized>();

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasPlannedV2>(
                        new Envelope(
                            buildingUnitWasPlannedV2,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() } })),
                    new Envelope<BuildingUnitWasNotRealizedBecauseBuildingWasNotRealized>(
                        new Envelope(
                            @event,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, @event.GetHash() } })))
                .Then(async ct =>
                {
                    var item = await ct.BuildingUnitDetailsV2.FindAsync(@event.BuildingUnitPersistentLocalId);
                    item.Should().NotBeNull();

                    item!.Status.Should().Be(BuildingUnitStatus.NotRealized);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasCorrectedFromNotRealizedToPlanned()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitWasNotRealizedV2 = _fixture.Create<BuildingUnitWasNotRealizedV2>();
            var @event = _fixture.Create<BuildingUnitWasCorrectedFromNotRealizedToPlanned>();

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasPlannedV2>(
                        new Envelope(
                            buildingUnitWasPlannedV2,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() } })),
                    new Envelope<BuildingUnitWasNotRealizedV2>(
                        new Envelope(
                            buildingUnitWasNotRealizedV2,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, buildingUnitWasNotRealizedV2.GetHash() } })),
                    new Envelope<BuildingUnitWasCorrectedFromNotRealizedToPlanned>(
                        new Envelope(
                            @event,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, @event.GetHash() } })))
                .Then(async ct =>
                {
                    var item = await ct.BuildingUnitDetailsV2.FindAsync(@event.BuildingUnitPersistentLocalId);
                    item.Should().NotBeNull();

                    item!.Status.Should().Be(BuildingUnitStatus.Planned);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasRetiredV2()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitWasRealizedV2 = _fixture.Create<BuildingUnitWasRealizedV2>();
            var @event = _fixture.Create<BuildingUnitWasRetiredV2>();

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasPlannedV2>(
                        new Envelope(
                            buildingUnitWasPlannedV2,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() } })),
                    new Envelope<BuildingUnitWasRealizedV2>(
                        new Envelope(
                            buildingUnitWasRealizedV2,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, buildingUnitWasRealizedV2.GetHash() } })),
                    new Envelope<BuildingUnitWasRetiredV2>(
                        new Envelope(
                            @event,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, @event.GetHash() } })))
                .Then(async ct =>
                {
                    var item = await ct.BuildingUnitDetailsV2.FindAsync(@event.BuildingUnitPersistentLocalId);
                    item.Should().NotBeNull();

                    item!.Status.Should().Be(BuildingUnitStatus.Retired);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasCorrectedFromRetiredToRealized()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitWasRetiredV2 = _fixture.Create<BuildingUnitWasRetiredV2>();
            var @event = _fixture.Create<BuildingUnitWasCorrectedFromRetiredToRealized>();

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasPlannedV2>(
                        new Envelope(
                            buildingUnitWasPlannedV2,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() } })),
                    new Envelope<BuildingUnitWasRetiredV2>(
                        new Envelope(
                            buildingUnitWasRetiredV2,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, buildingUnitWasRetiredV2.GetHash() } })),
                    new Envelope<BuildingUnitWasCorrectedFromRetiredToRealized>(
                        new Envelope(
                            @event,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, @event.GetHash() } })))
                .Then(async ct =>
                {
                    var item = await ct.BuildingUnitDetailsV2.FindAsync(@event.BuildingUnitPersistentLocalId);
                    item.Should().NotBeNull();

                    item!.Status.Should().Be(BuildingUnitStatus.Realized);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitPositionWasCorrected()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();

            ((ISetProvenance)buildingUnitWasPlannedV2).SetProvenance(_fixture.Create<Provenance>());


            var @event = new BuildingUnitPositionWasCorrected(
               new BuildingPersistentLocalId(buildingUnitWasPlannedV2.BuildingPersistentLocalId),
                new BuildingUnitPersistentLocalId(buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId),
                BuildingUnitPositionGeometryMethod.Parse("AppointedByAdministrator"),
               new ExtendedWkbGeometry(GeometryHelper.ValidPointInPolygon.AsBinary()));
            ((ISetProvenance)@event).SetProvenance(_fixture.Create<Provenance>());

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasPlannedV2>(
                        new Envelope(
                            buildingUnitWasPlannedV2,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() } })),
                  new Envelope<BuildingUnitPositionWasCorrected>(
                        new Envelope(
                            @event,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, @event.GetHash() } })))
                .Then(async ct =>
                {
                    var item = await ct.BuildingUnitDetailsV2.FindAsync(@event.BuildingUnitPersistentLocalId);
                    item.Should().NotBeNull();
                    item.BuildingPersistentLocalId.Should().Be(@event.BuildingPersistentLocalId);
                    item.Position.Should().BeEquivalentTo(@event.ExtendedWkbGeometry.ToByteArray());
                    item.PositionMethod.Should().Be(BuildingUnitPositionGeometryMethod.Parse(@event.GeometryMethod));
                    item.Version.Should().Be(@event.Provenance.Timestamp);
                    item.IsRemoved.Should().BeFalse();
                    item.Status.Should().Be(BuildingUnitStatus.Planned);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasRemovedV2()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();

            ((ISetProvenance)buildingUnitWasPlannedV2).SetProvenance(_fixture.Create<Provenance>());


            var @event = new BuildingUnitWasRemovedV2(
               new BuildingPersistentLocalId(buildingUnitWasPlannedV2.BuildingPersistentLocalId),
                new BuildingUnitPersistentLocalId(buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId));
            ((ISetProvenance)@event).SetProvenance(_fixture.Create<Provenance>());

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasPlannedV2>(
                        new Envelope(
                            buildingUnitWasPlannedV2,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() } })),
                  new Envelope<BuildingUnitWasRemovedV2>(
                        new Envelope(
                            @event,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, @event.GetHash() } })))
                .Then(async ct =>
                {
                    var item = await ct.BuildingUnitDetailsV2.FindAsync(@event.BuildingUnitPersistentLocalId);
                    item.Should().NotBeNull();
                    item!.Version.Should().Be(@event.Provenance.Timestamp);
                    item.IsRemoved.Should().BeTrue();
                });
        }

        [Fact]
        public async Task WhenBuildingUnitRemovalWasCorrected()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitWasRemovedV2 = _fixture.Create<BuildingUnitWasRemovedV2>();
            var @event = _fixture.Create<BuildingUnitRemovalWasCorrected>();

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasPlannedV2>(
                        new Envelope(
                            buildingUnitWasPlannedV2,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() } })),
                  new Envelope<BuildingUnitWasRemovedV2>(
                        new Envelope(
                            buildingUnitWasRemovedV2,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, buildingUnitWasRemovedV2.GetHash() } })),
                  new Envelope<BuildingUnitRemovalWasCorrected>(
                        new Envelope(
                            @event,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, @event.GetHash() } })))
                .Then(async ct =>
                {
                    var item = await ct.BuildingUnitDetailsV2.FindAsync(@event.BuildingUnitPersistentLocalId);
                    item.Should().NotBeNull();
                    item!.Status.Should().Be(BuildingUnitStatus.Parse(@event.BuildingUnitStatus));
                    item.Function.Should().Be(BuildingUnitFunction.Parse(@event.Function));
                    item.Position.Should().BeEquivalentTo(@event.ExtendedWkbGeometry.ToByteArray());
                    item.PositionMethod.Should().Be(BuildingUnitPositionGeometryMethod.Parse(@event.GeometryMethod));
                    item.IsRemoved.Should().BeFalse();
                    item.Version.Should().Be(@event.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenCommonBuildingUnitWasAddedV2()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var commonBuildingUnitWasAddedV2 = new CommonBuildingUnitWasAddedV2(
                _fixture.Create<BuildingPersistentLocalId>(),
                _fixture.Create<BuildingUnitPersistentLocalId>(),
                BuildingUnitStatus.Planned,
                BuildingUnitPositionGeometryMethod.DerivedFromObject,
                _fixture.Create<ExtendedWkbGeometry>(),
                false);
            ((ISetProvenance)commonBuildingUnitWasAddedV2).SetProvenance(_fixture.Create<Provenance>());

            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, commonBuildingUnitWasAddedV2.GetHash() }
            };

            await Sut
                .Given(new Envelope<CommonBuildingUnitWasAddedV2>(new Envelope(commonBuildingUnitWasAddedV2, metadata)))
                .Then(async ct =>
                {
                    var item = await ct.BuildingUnitDetailsV2.FindAsync(commonBuildingUnitWasAddedV2.BuildingUnitPersistentLocalId);
                    item.Should().NotBeNull();
                    item.BuildingPersistentLocalId.Should().Be(commonBuildingUnitWasAddedV2.BuildingPersistentLocalId);
                    item.Position.Should().BeEquivalentTo(commonBuildingUnitWasAddedV2.ExtendedWkbGeometry.ToByteArray());
                    item.PositionMethod.Should().Be(BuildingUnitPositionGeometryMethod.Parse(commonBuildingUnitWasAddedV2.GeometryMethod));
                    item.Function.Should()
                        .Be(BuildingUnitFunction.Common);
                    item.Version.Should().Be(commonBuildingUnitWasAddedV2.Provenance.Timestamp);
                    item.IsRemoved.Should().BeFalse();
                    item.Status.Should().Be(BuildingUnitStatus.Parse(commonBuildingUnitWasAddedV2.BuildingUnitStatus));
                });
        }

        protected override BuildingUnitDetailV2Projections CreateProjection() => new BuildingUnitDetailV2Projections();
    }
}
