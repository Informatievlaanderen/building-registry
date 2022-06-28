namespace BuildingRegistry.Tests.BackOffice.Handlers.Building
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
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
        private PlanBuildingUnitHandler _sut;

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
        public async Task WhenCorrectRequestIsSent_ThenResponseIsExpected()
        {
            var buildingPersistentLocalId = new BuildingPersistentLocalId(123);

            DispatchArrangeCommand(new PlanBuilding(
                buildingPersistentLocalId,
                Fixture.Create<ExtendedWkbGeometry>(),
                Fixture.Create<Provenance>()));

            var generateNextPersistentLocalId = _fakePersistentLocalIdGenerator.GenerateNextPersistentLocalId();

            var request = new PlanBuildingUnitRequest
            {
                GebouwId = $"https://data.vlaanderen.be/id/gebouw/{buildingPersistentLocalId}",
                PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                Positie = "<gml:Point srsName=\"https://www.opengis.net/def/crs/EPSG/0/3137\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>103671.37 192046.71</gml:pos></gml:Point>",
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
