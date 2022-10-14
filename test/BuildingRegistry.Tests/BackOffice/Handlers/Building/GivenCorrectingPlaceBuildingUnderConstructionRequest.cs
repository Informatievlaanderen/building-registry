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
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Api.BackOffice.Handlers.Building;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using Fixtures;
    using FluentAssertions;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using Xunit;
    using Xunit.Abstractions;
    using BuildingUnit = BuildingRegistry.Building.Commands.BuildingUnit;

    public class GivenCorrectingPlaceBuildingUnderConstructionRequest : BuildingRegistryTest
    {
        private readonly CorrectPlaceBuildingUnderConstructionHandler _sut;

        private readonly IBuildings _repo;

        private readonly BuildingPersistentLocalId _buildingPersistentLocalId;

        private BuildingStreamId BuildingStreamId => new BuildingStreamId(_buildingPersistentLocalId);

        public GivenCorrectingPlaceBuildingUnderConstructionRequest(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();

            _repo = Container.Resolve<IBuildings>();

            _sut = new CorrectPlaceBuildingUnderConstructionHandler(
                Container.Resolve<ICommandHandlerResolver>(),
                new FakeIdempotencyContextFactory().CreateDbContext(),
                _repo);
        }

        [Fact]
        public async Task WhenBuildingUnderConstruction_ThenBuildingPlanned()
        {
            var migrateBuilding = new MigrateBuilding(
                Fixture.Create<BuildingRegistry.Legacy.BuildingId>(),
                Fixture.Create<BuildingRegistry.Legacy.PersistentLocalId>(),
                Fixture.Create<BuildingRegistry.Legacy.PersistentLocalIdAssignmentDate>(),
                BuildingRegistry.Legacy.BuildingStatus.UnderConstruction,
                Fixture.Create<BuildingRegistry.Legacy.BuildingGeometry>(),
                isRemoved: false,
                new List<BuildingUnit>(),
                Fixture.Create<Provenance>()
            );
            DispatchArrangeCommand(migrateBuilding);

            var request = new CorrectPlaceBuildingUnderConstructionRequest
            {
                PersistentLocalId = _buildingPersistentLocalId
            };

            // Act
            var result = await _sut.Handle(request, CancellationToken.None);

            // Assert
            var actual = await _repo.GetAsync(BuildingStreamId);
            actual.Should().NotBeNull();
            actual.BuildingStatus.Should().Be(BuildingStatus.Planned);

            var stream = await Container.Resolve<IStreamStore>().ReadStreamBackwards(new StreamId(BuildingStreamId), fromVersionInclusive: 1, maxCount: 1); // 1 = fromVersionInclusive of stream (zero based)
            stream.Messages.First().JsonMetadata.Should().Contain(result.ETag);
        }

        [Fact]
        public async Task WhenBuildingPlanned_ThenBuildingPlanned()
        {
            var request = new CorrectPlaceBuildingUnderConstructionRequest
            {
                PersistentLocalId = _buildingPersistentLocalId
            };

            var migrateBuilding = new MigrateBuilding(
                Fixture.Create<BuildingRegistry.Legacy.BuildingId>(),
                Fixture.Create<BuildingRegistry.Legacy.PersistentLocalId>(),
                Fixture.Create<BuildingRegistry.Legacy.PersistentLocalIdAssignmentDate>(),
                BuildingRegistry.Legacy.BuildingStatus.Planned,
                Fixture.Create<BuildingRegistry.Legacy.BuildingGeometry>(),
                isRemoved: false,
                new List<BuildingUnit>(),
                Fixture.Create<Provenance>()
            );
            DispatchArrangeCommand(migrateBuilding);

            // Act
            var result = await _sut.Handle(request, CancellationToken.None);

            // Assert
            var actual = await _repo.GetAsync(BuildingStreamId);
            actual.Should().NotBeNull();
            actual.BuildingStatus.Should().Be(BuildingStatus.Planned);

            var stream = await Container.Resolve<IStreamStore>().ReadStreamBackwards(new StreamId(BuildingStreamId), fromVersionInclusive: 0, maxCount: 1); // 1 = fromVersionInclusive of stream (zero based)
            stream.Messages.First().JsonMetadata.Should().Contain(result.ETag);
        }
    }
}
