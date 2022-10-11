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
            if (expectedResult)
            {
                var expectedPattern = Pattern.EndsWith(new BuildingStreamId(buildingPersistentLocalId).ToString());
                streamStoreMock
                    .Setup(store => store.ListStreams(It.Is<Pattern>(x => x.Value == expectedPattern.Value), 1, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(() => new ListStreamsPage("1", new[] { buildingPersistentLocalId.ToString() }, (_, _) => null));
            }

            var sut = new BuildingExistsValidator(streamStoreMock.Object);

            var result = await sut.Exists(buildingPersistentLocalId, CancellationToken.None);

            result.Should().Be(expectedResult);
        }
    }
}
