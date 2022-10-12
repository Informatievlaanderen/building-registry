namespace BuildingRegistry.Tests.BackOffice
{
    using System.Threading;
    using AutoFixture;
    using Moq;
    using SqlStreamStore;
    using SqlStreamStore.Streams;

    public static class MockStreamStoreExtensions
    {
        public static void SetStreamFound(this Mock<IStreamStore> streamStoreMock)
        {
            streamStoreMock
                .Setup(store => store.GetStreamMetadata(It.IsAny<string>(), CancellationToken.None))
                .ReturnsAsync(() => new StreamMetadataResult(new Fixture().Create<string>(), 1));
        }

        public static void SetStreamNotFound(this Mock<IStreamStore> streamStoreMock)
        {
            streamStoreMock
                .Setup(store => store.GetStreamMetadata(It.IsAny<string>(), CancellationToken.None))
                .ReturnsAsync(() => new StreamMetadataResult(new Fixture().Create<string>(), -1));
        }
    }
}
