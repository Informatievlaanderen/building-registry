namespace BuildingRegistry.Tests.BackOffice.Validators
{
    using System;
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
            var streamId = new StreamId(new BuildingStreamId(buildingPersistentLocalId));

            streamStoreMock
                .Setup(store => store.ReadStreamBackwards(streamId, StreamVersion.End, 1, false, CancellationToken.None))
                .ReturnsAsync(() => expectedResult
                    ? new ReadStreamPage(streamId, PageReadStatus.Success, 1, 2, 2, 2, ReadDirection.Backward, false, messages: new []{ new StreamMessage() })
                    : new ReadStreamPage(streamId, PageReadStatus.StreamNotFound, -1, -1, -1, -1, ReadDirection.Backward, false, messages: Array.Empty<StreamMessage>()));

            var sut = new BuildingExistsValidator(streamStoreMock.Object);

            var result = await sut.Exists(buildingPersistentLocalId, CancellationToken.None);

            result.Should().Be(expectedResult);
        }
    }
}
