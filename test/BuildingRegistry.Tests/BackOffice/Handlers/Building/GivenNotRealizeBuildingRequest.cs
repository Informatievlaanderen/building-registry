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
    using BuildingRegistry.Legacy;
    using Fixtures;
    using FluentAssertions;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using Xunit;
    using Xunit.Abstractions;
    using BuildingGeometry = BuildingRegistry.Legacy.BuildingGeometry;
    using BuildingId = BuildingRegistry.Legacy.BuildingId;
    using BuildingStatus = BuildingRegistry.Legacy.BuildingStatus;
    using BuildingUnit = BuildingRegistry.Building.Commands.BuildingUnit;
    using IBuildings = BuildingRegistry.Building.IBuildings;

    public class GivenNotRealizeBuildingRequest : BuildingRegistryTest
    {
        private readonly NotRealizeBuildingHandler _sut;

        private readonly IBuildings _repo;

        private readonly BuildingPersistentLocalId _buildingPersistentLocalId;

        private BuildingStreamId BuildingStreamId => new BuildingStreamId(_buildingPersistentLocalId);

        public GivenNotRealizeBuildingRequest(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();

            _repo = Container.Resolve<IBuildings>();

            _sut = new NotRealizeBuildingHandler(
                Container.Resolve<ICommandHandlerResolver>(),
                new FakeIdempotencyContextFactory().CreateDbContext(),
                _repo);
        }

        [Fact]
        public async Task WhenBuildingUnderConstruction_ThenBuildingWasNotRealized()
        {
            Fixture.Register(() => BuildingStatus.UnderConstruction);

            var migrateBuilding = new MigrateBuilding(
                Fixture.Create<BuildingId>(),
                Fixture.Create<PersistentLocalId>(),
                Fixture.Create<PersistentLocalIdAssignmentDate>(),
                BuildingStatus.UnderConstruction,
                Fixture.Create<BuildingGeometry>(),
                isRemoved: false,
                new List<BuildingUnit>(),
                Fixture.Create<Provenance>()
            );
            DispatchArrangeCommand(migrateBuilding);

            var request = new NotRealizeBuildingRequest
            {
                PersistentLocalId = _buildingPersistentLocalId
            };

            // Act
            var result = await _sut.Handle(request, CancellationToken.None);

            // Assert
            var actual = await _repo.GetAsync(BuildingStreamId);
            actual.Should().NotBeNull();
            actual.BuildingStatus.Should().Be(BuildingRegistry.Building.BuildingStatus.NotRealized);

            var stream = await Container.Resolve<IStreamStore>().ReadStreamBackwards(new StreamId(BuildingStreamId), fromVersionInclusive: 1, maxCount: 1); // 1 = fromVersionInclusive of stream (zero based)
            stream.Messages.First().JsonMetadata.Should().Contain(result.ETag);
        }

        [Fact]
        public async Task WhenBuildingNotRealized_ThenBuildingWasNotRealized()
        {
            Fixture.Register(() => BuildingStatus.NotRealized);

            var migrateBuilding = new MigrateBuilding(
                Fixture.Create<BuildingId>(),
                Fixture.Create<PersistentLocalId>(),
                Fixture.Create<PersistentLocalIdAssignmentDate>(),
                BuildingStatus.NotRealized,
                Fixture.Create<BuildingGeometry>(),
                isRemoved: false,
                new List<BuildingUnit>(),
                Fixture.Create<Provenance>()
            );
            DispatchArrangeCommand(migrateBuilding);

            var request = new NotRealizeBuildingRequest
            {
                PersistentLocalId = _buildingPersistentLocalId
            };

            // Act
            var result = await _sut.Handle(request, CancellationToken.None);

            // Assert
            var actual = await _repo.GetAsync(BuildingStreamId);
            actual.Should().NotBeNull();
            actual.BuildingStatus.Should().Be(BuildingRegistry.Building.BuildingStatus.NotRealized);

            var stream = await Container.Resolve<IStreamStore>().ReadStreamBackwards(new StreamId(BuildingStreamId), fromVersionInclusive: 0, maxCount: 1); // 1 = fromVersionInclusive of stream (zero based)
            stream.Messages.First().JsonMetadata.Should().Contain(result.ETag);
        }
    }
}
