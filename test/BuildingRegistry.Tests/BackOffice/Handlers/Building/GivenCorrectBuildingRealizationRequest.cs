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
    using Xunit;
    using Xunit.Abstractions;
    using BuildingGeometry = BuildingRegistry.Legacy.BuildingGeometry;
    using BuildingGeometryMethod = BuildingRegistry.Legacy.BuildingGeometryMethod;
    using BuildingId = BuildingRegistry.Legacy.BuildingId;
    using BuildingStatus = BuildingRegistry.Legacy.BuildingStatus;
    using BuildingUnit = BuildingRegistry.Building.Commands.BuildingUnit;
    using ExtendedWkbGeometry = BuildingRegistry.Legacy.ExtendedWkbGeometry;
    using IBuildings = BuildingRegistry.Building.IBuildings;

    public class GivenCorrectBuildingRealizationRequest : BuildingRegistryTest
    {
        private readonly CorrectBuildingRealizationHandler _sut;
        private readonly IBuildings _buildings;

        public GivenCorrectBuildingRealizationRequest(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());

            _buildings = Container.Resolve<IBuildings>();

            _sut = new CorrectBuildingRealizationHandler(
                Container.Resolve<ICommandHandlerResolver>(),
                new FakeIdempotencyContextFactory().CreateDbContext(),
                _buildings);
        }

        [Fact]
        public async Task ThenPersistentLocalIdETagResponse()
        {
            DispatchArrangeCommand(new MigrateBuilding(
                Fixture.Create<BuildingId>(),
                Fixture.Create<PersistentLocalId>(),
                Fixture.Create<PersistentLocalIdAssignmentDate>(),
                BuildingStatus.Realized,
                new  BuildingGeometry(
                    Fixture.Create<ExtendedWkbGeometry>(),
                    BuildingGeometryMethod.Outlined),
                isRemoved: false,
                new List<BuildingUnit>(),
                Fixture.Create<Provenance>()
            ));

            // Act
            var result = await _sut.Handle(
                new CorrectBuildingRealizationRequest { PersistentLocalId = Fixture.Create<BuildingPersistentLocalId>() },
                CancellationToken.None);

            // Assert
            var buildingStreamId = new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>());
            var actual = await _buildings.GetAsync(buildingStreamId);
            actual.Should().NotBeNull();
            actual.BuildingStatus.Should().Be(BuildingRegistry.Building.BuildingStatus.UnderConstruction);

            var stream = await Container.Resolve<IStreamStore>().ReadStreamBackwards(buildingStreamId, 1, 1);
            stream.Messages.First().JsonMetadata.Should().Contain(result.ETag);
        }
    }
}
