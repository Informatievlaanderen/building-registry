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
    using FluentAssertions;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenRegularizeBuildingUnitRequest : BuildingRegistryTest
    {
        private readonly RegularizeBuildingUnitHandler _sut;
        private readonly BackOfficeContext _backOfficeContext;

        public GivenRegularizeBuildingUnitRequest(ITestOutputHelper outputHelper) : base(outputHelper)
        {
            _backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext();
            _sut = new RegularizeBuildingUnitHandler(
                Container.Resolve<ICommandHandlerResolver>(),
                Container.Resolve<IBuildings>(),
                _backOfficeContext,
                new FakeIdempotencyContextFactory().CreateDbContext());
        }

        [Fact]
        public async Task GivenBuildingUnitPlanned_ThenBuildingUnitRegularized()
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

            DispatchArrangeCommand(new PlanBuildingUnit(
                buildingPersistentLocalId,
                buildingUnitPersistentLocalId,
                Fixture.Create<BuildingUnitPositionGeometryMethod>(),
                Fixture.Create<ExtendedWkbGeometry>(),
                Fixture.Create<BuildingUnitFunction>(),
                hasDeviation: true,
                Fixture.Create<Provenance>()));

            var request = new RegularizeBuildingUnitRequest
            {
                BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId
            };

            var response = await _sut.Handle(request, CancellationToken.None);

            response.Should().NotBeNull();
            var stream = await Container.Resolve<IStreamStore>().ReadStreamBackwards(new StreamId(new BuildingStreamId(buildingPersistentLocalId)), 2, 1);
            stream.Messages.First().JsonMetadata.Should().Contain(response.ETag);
        }
    }
}
