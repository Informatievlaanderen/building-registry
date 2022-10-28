namespace BuildingRegistry.Tests.BackOffice.Handlers.Building
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Api.BackOffice.Handlers.Building;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using BuildingRegistry.Legacy;
    using Fixtures;
    using FluentAssertions;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using Xunit;
    using Xunit.Abstractions;
    using BuildingGeometry = BuildingRegistry.Legacy.BuildingGeometry;
    using BuildingGeometryMethod = BuildingRegistry.Building.BuildingGeometryMethod;
    using BuildingId = BuildingRegistry.Legacy.BuildingId;
    using BuildingStatus = BuildingRegistry.Legacy.BuildingStatus;
    using BuildingUnit = BuildingRegistry.Building.Commands.BuildingUnit;
    using IBuildings = BuildingRegistry.Building.IBuildings;

    public class GivenChangeBuildingOutlineRequest : BuildingRegistryTest
    {
        private readonly ChangeBuildingOutlineHandler _sut;

        private readonly IBuildings _repo;

        private readonly BuildingPersistentLocalId _buildingPersistentLocalId;

        private BuildingStreamId BuildingStreamId => new BuildingStreamId(_buildingPersistentLocalId);

        public GivenChangeBuildingOutlineRequest(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();

            _repo = Container.Resolve<IBuildings>();

            _sut = new ChangeBuildingOutlineHandler(
                Container.Resolve<ICommandHandlerResolver>(),
                new FakeIdempotencyContextFactory().CreateDbContext(),
                _repo);
        }

        [Fact]
        public async Task ThenBuildingOutlineWasChanged()
        {
            Fixture.Register(() => BuildingStatus.UnderConstruction);

            var migrateBuilding = new MigrateBuilding(
                Fixture.Create<BuildingId>(),
                Fixture.Create<PersistentLocalId>(),
                Fixture.Create<PersistentLocalIdAssignmentDate>(),
                BuildingStatus.Planned,
                new BuildingGeometry(
                    new BuildingRegistry.Legacy.ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()),
                    BuildingRegistry.Legacy.BuildingGeometryMethod.Outlined),
                isRemoved: false,
                new List<BuildingUnit>(),
                Fixture.Create<Provenance>()
            );
            DispatchArrangeCommand(migrateBuilding);

            var request = new ChangeBuildingOutlineRequest
            {
                PersistentLocalId = _buildingPersistentLocalId,
                GeometriePolygoon =
                    "<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:exterior><gml:LinearRing><gml:posList>140284.15277253836 186724.74131567031 140291.06016454101 186726.38355567306 140288.22675654292 186738.25798767805 140281.19098053873 186736.57913967967 140284.15277253836 186724.74131567031</gml:posList></gml:LinearRing></gml:exterior></gml:Polygon>"
            };

            // Act
            var result = await _sut.Handle(request, CancellationToken.None);

            // Assert
            var actual = await _repo.GetAsync(BuildingStreamId);
            actual.Should().NotBeNull();
            actual.BuildingGeometry.Should().Be(
                new BuildingRegistry.Building.BuildingGeometry(
                    request.GeometriePolygoon.ToExtendedWkbGeometry(),
                    BuildingGeometryMethod.Outlined));

            var stream = await Container.Resolve<IStreamStore>().ReadStreamBackwards(new StreamId(BuildingStreamId), fromVersionInclusive: 1, maxCount: 1);
            stream.Messages.First().JsonMetadata.Should().Contain(result.ETag);
        }
    }
}
