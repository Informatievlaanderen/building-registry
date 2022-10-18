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

    public class GivenCorrectBuildingUnitNotRealizationRequest : BuildingRegistryTest
    {
        private readonly CorrectBuildingUnitNotRealizationHandler _sut;
        private readonly BackOfficeContext _backOfficeContext;

        public GivenCorrectBuildingUnitNotRealizationRequest(ITestOutputHelper outputHelper) : base(outputHelper)
        {
            _backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext();
            _sut = new CorrectBuildingUnitNotRealizationHandler(
                Container.Resolve<ICommandHandlerResolver>(),
                Container.Resolve<IBuildings>(),
                _backOfficeContext,
                new FakeIdempotencyContextFactory().CreateDbContext());
        }

        [Fact]
        public async Task GivenBuildingUnitNotRealized_ThenBuildingUnitIsCorrectedToPlanned()
        {
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();
            var buildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();

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

            DispatchArrangeCommand(new RealizeBuilding(
                buildingPersistentLocalId,
                Fixture.Create<Provenance>()));

            DispatchArrangeCommand(new PlanBuildingUnit(
                buildingPersistentLocalId,
                buildingUnitPersistentLocalId,
                Fixture.Create<BuildingUnitPositionGeometryMethod>(),
                Fixture.Create<ExtendedWkbGeometry>(),
                BuildingUnitFunction.Unknown,
                hasDeviation: false,
                Fixture.Create<Provenance>()));

            DispatchArrangeCommand(new NotRealizeBuildingUnit(
                buildingPersistentLocalId,
                buildingUnitPersistentLocalId,
                Fixture.Create<Provenance>()));

            var request = new CorrectBuildingUnitNotRealizationRequest
            {
                BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId
            };

            var response = await _sut.Handle(request, CancellationToken.None);

            response.Should().NotBeNull();
            var stream = await Container.Resolve<IStreamStore>().ReadStreamBackwards(new StreamId(new BuildingStreamId(buildingPersistentLocalId)), 5, 1);
            stream.Messages.First().JsonMetadata.Should().Contain(response.ETag);
        }
    }
}
