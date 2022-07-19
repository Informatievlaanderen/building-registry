namespace BuildingRegistry.Tests.BackOffice.Handlers.BuildingUnit
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.Handlers.BuildingUnit;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using FluentAssertions;
    using Infrastructure;
    using Legacy;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenPlanBuildingUnitRequest : BuildingRegistryTest
    {
        private readonly FakePersistentLocalIdGenerator _fakePersistentLocalIdGenerator = new FakePersistentLocalIdGenerator();
        private readonly PlanBuildingUnitHandler _sut;

        public GivenPlanBuildingUnitRequest(ITestOutputHelper outputHelper) : base(outputHelper)
        {
            _sut = new PlanBuildingUnitHandler(
                Container.Resolve<ICommandHandlerResolver>(),
                Container.Resolve<IBuildings>(),
                new FakeBackOfficeContextFactory().CreateDbContext(),
                _fakePersistentLocalIdGenerator,
                new FakeIdempotencyContextFactory().CreateDbContext());
        }

        [Fact]
        public async Task WhenExistingBuilding_ThenBuildingUnitIsPlanned()
        {
            var buildingPersistentLocalId = new BuildingPersistentLocalId(123);
            var geometry = "" +
                           "<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\">" +
                           "<gml:exterior>" +
                           "<gml:LinearRing>" +
                           "<gml:posList>140284.15277253836 186724.74131567031 140291.06016454101 186726.38355567306 140288.22675654292 186738.25798767805 140281.19098053873 186736.57913967967 140284.15277253836 186724.74131567031</gml:posList>" +
                           "</gml:LinearRing>" +
                           "</gml:exterior>" +
                           "</gml:Polygon>";

            DispatchArrangeCommand(new PlanBuilding(
                buildingPersistentLocalId,
                geometry.ToExtendedWkbGeometry(),
                Fixture.Create<Provenance>()));

            var generateNextPersistentLocalId = _fakePersistentLocalIdGenerator.GenerateNextPersistentLocalId();

            var request = new PlanBuildingUnitRequest
            {
                GebouwId = $"https://data.vlaanderen.be/id/gebouw/{buildingPersistentLocalId}",
                PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                Positie = "<gml:Point srsName=\"https://www.opengis.net/def/crs/EPSG/0/3137\" xmlns:gml=\"http://www.opengis.net/gml/3.2\">" +
                          "<gml:pos>140285.15277253836 186725.74131567031</gml:pos></gml:Point>",
                Functie = GebouweenheidFunctie.NietGekend,
                AfwijkingVastgesteld = false
            };

            var response = await _sut.Handle(request, CancellationToken.None);

            response.Should().NotBeNull();
            response.BuildingUnitPersistentLocalId.Should().Be(generateNextPersistentLocalId);

            var stream = await Container.Resolve<IStreamStore>().ReadStreamBackwards(new StreamId(new BuildingStreamId(buildingPersistentLocalId)), 1, 1);
            stream.Messages.First().JsonMetadata.Should().Contain(response.LastEventHash);
        }
    }
}
