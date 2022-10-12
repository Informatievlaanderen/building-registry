namespace BuildingRegistry.Tests.BackOffice.Validators
{
    using System.Threading;
    using System.Threading.Tasks;
    using Building;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators;
    using FluentAssertions;
    using Moq;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using Xunit;

    public class BuildingExistsValidatorTests
    {
        [Theory]
        [InlineData(1, true)]
        [InlineData(2, false)]
        public async Task GivenId_ThenReturnsExpectedResult(int buildingId, bool expectedResult)
        {
            var streamStoreMock = new Mock<IStreamStore>();

            var buildingPersistentLocalId = new BuildingPersistentLocalId(buildingId);
            var streamId = new BuildingStreamId(buildingPersistentLocalId).ToString();

            streamStoreMock
                .Setup(store => store.GetStreamMetadata(streamId, CancellationToken.None))
                .ReturnsAsync(() => expectedResult ? new StreamMetadataResult(streamId, 1) : new StreamMetadataResult(streamId, -1));

            var sut = new BuildingExistsValidator(streamStoreMock.Object);

            var result = await sut.Exists(buildingPersistentLocalId, CancellationToken.None);

            result.Should().Be(expectedResult);
        }
    }
}
