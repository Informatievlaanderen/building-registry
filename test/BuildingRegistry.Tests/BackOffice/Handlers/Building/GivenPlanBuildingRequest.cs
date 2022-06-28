namespace BuildingRegistry.Tests.BackOffice.Handlers.Building
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Api.BackOffice.Handlers.Building;
    using BuildingRegistry.Building;
    using FluentAssertions;
    using Infrastructure;
    using Legacy;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenPlanBuildingRequest : BuildingRegistryTest
    {
        private readonly FakePersistentLocalIdGenerator _fakePersistentLocalIdGenerator = new FakePersistentLocalIdGenerator();
        private readonly PlanBuildingHandler _sut;

        public GivenPlanBuildingRequest(ITestOutputHelper outputHelper) : base(outputHelper)
        {
            _sut = new PlanBuildingHandler(
                Container.Resolve<ICommandHandlerResolver>(),
                new FakeIdempotencyContextFactory().CreateDbContext(),
                Container.Resolve<IBuildings>(),
                _fakePersistentLocalIdGenerator);
        }

        [Fact]
        public async Task WhenCorrectRequestIsSent_ThenResponseIsExpected()
        {
            var generateNextPersistentLocalId = _fakePersistentLocalIdGenerator.GenerateNextPersistentLocalId();

            var planBuildingRequest = new PlanBuildingRequest
            {
                GeometriePolygoon = "<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:exterior><gml:LinearRing><gml:posList>140284.15277253836 186724.74131567031 140291.06016454101 186726.38355567306 140288.22675654292 186738.25798767805 140281.19098053873 186736.57913967967 140284.15277253836 186724.74131567031</gml:posList></gml:LinearRing></gml:exterior></gml:Polygon>",
                Metadata = new Dictionary<string, object>()
            };

            var response = await _sut.Handle(planBuildingRequest, CancellationToken.None);

            response.Should().NotBeNull();
            
            response.BuildingPersistentLocalId.Should().Be(generateNextPersistentLocalId);
            
            var stream = await Container.Resolve<IStreamStore>().ReadStreamBackwards(new StreamId(new BuildingStreamId(new BuildingPersistentLocalId(generateNextPersistentLocalId))), 0, 1); //1 = version of stream (zero based)
            stream.Messages.First().JsonMetadata.Should().Contain(response.LastEventHash);
        }
    }
}
