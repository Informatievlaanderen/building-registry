namespace BuildingRegistry.Tests.BackOffice.Handlers.BuildingUnit
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.Handlers.BuildingUnit;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using FluentAssertions;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenDeregulateBuildingUnitRequest : BuildingRegistryTest
    {
        private readonly DeregulateBuildingUnitHandler _sut;
        private readonly FakeBackOfficeContext _backOfficeContext;

        public GivenDeregulateBuildingUnitRequest(ITestOutputHelper outputHelper) : base(outputHelper)
        {
            _backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext();
            _sut = new DeregulateBuildingUnitHandler(
                Container.Resolve<ICommandHandlerResolver>(),
                Container.Resolve<IBuildings>(),
                _backOfficeContext,
                new FakeIdempotencyContextFactory().CreateDbContext());
        }

        [Fact]
        public async Task GivenBuildingUnitPlanned_ThenBuildingUnitDeregulated()
        {
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();
            var buildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();

            await _backOfficeContext.AddBuildingUnitBuilding(buildingPersistentLocalId, buildingUnitPersistentLocalId);

            DispatchArrangeCommand(new PlanBuilding(
                buildingPersistentLocalId,
                Fixture.Create<ExtendedWkbGeometry>(),
                Fixture.Create<Provenance>()));

            DispatchArrangeCommand(new PlanBuildingUnit(
                buildingPersistentLocalId,
                buildingUnitPersistentLocalId,
                Fixture.Create<BuildingUnitPositionGeometryMethod>(),
                Fixture.Create<ExtendedWkbGeometry>(),
                BuildingUnitFunction.Unknown,
                hasDeviation: false,
                Fixture.Create<Provenance>()));

            var response = await _sut.Handle(
                new DeregulateBuildingUnitRequest { BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId },
                CancellationToken.None);

            response.Should().NotBeNull();
            var stream = await Container.Resolve<IStreamStore>().ReadStreamBackwards(new StreamId(new BuildingStreamId(buildingPersistentLocalId)), 2, 1);
            stream.Messages.First().JsonMetadata.Should().Contain(response.ETag);
        }
    }
}
