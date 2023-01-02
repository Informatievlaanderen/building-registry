namespace BuildingRegistry.Tests.BackOffice.Handlers.BuildingUnit
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Api.BackOffice.Abstractions;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.Handlers.BuildingUnit;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using BuildingRegistry.Building.Events;
    using FluentAssertions;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenRemoveBuildingUnitRequest : BuildingRegistryTest
    {
        private readonly RemoveBuildingUnitHandler _sut;
        private readonly BackOfficeContext _backOfficeContext;

        public GivenRemoveBuildingUnitRequest(ITestOutputHelper outputHelper) : base(outputHelper)
        {
            _backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext();
            _sut = new RemoveBuildingUnitHandler(
                Container.Resolve<ICommandHandlerResolver>(),
                Container.Resolve<IBuildings>(),
                _backOfficeContext,
                new FakeIdempotencyContextFactory().CreateDbContext());
        }

        [Fact]
        public async Task GivenBuildingUnitPlanned_ThenBuildingUnitRemoved()
        {
            var buildingPersistentLocalId = new BuildingPersistentLocalId(123);
            var buildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(456);

            _backOfficeContext.BuildingUnitBuildings.Add(new BuildingUnitBuilding(
                buildingUnitPersistentLocalId,
                buildingPersistentLocalId));

            DispatchArrangeCommand(new PlanBuilding(
                buildingPersistentLocalId,
                Fixture.Create<ExtendedWkbGeometry>(),
                Fixture.Create<Provenance>()));

            DispatchArrangeCommand(new PlaceBuildingUnderConstruction(
                buildingPersistentLocalId,
                Fixture.Create<Provenance>()));

            DispatchArrangeCommand(new PlanBuildingUnit(
                buildingPersistentLocalId,
                buildingUnitPersistentLocalId,
                Fixture.Create<BuildingUnitPositionGeometryMethod>(),
                Fixture.Create<ExtendedWkbGeometry>(),
                Fixture.Create<BuildingUnitFunction>(),
                hasDeviation: false,
                Fixture.Create<Provenance>()));

            var request = new RemoveBuildingUnitRequest
            {
                BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId
            };

            var response = await _sut.Handle(request, CancellationToken.None);

            response.Should().NotBeNull();
            var stream = await Container.Resolve<IStreamStore>().ReadStreamBackwards(new StreamId(new BuildingStreamId(buildingPersistentLocalId)), 3, 1);
            stream.Messages.First().Type.Should().Be(BuildingUnitWasRemovedV2.EventName);
        }
    }
}