namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators
{
    using System.Threading;
    using System.Threading.Tasks;
    using BuildingRegistry.Building;
    using SqlStreamStore;
    using SqlStreamStore.Streams;

    public sealed class BuildingExistsValidator
    {
        private readonly IStreamStore _streamStore;

        public BuildingExistsValidator(IStreamStore streamStore)
        {
            _streamStore = streamStore;
        }

        public async Task<bool> Exists(BuildingPersistentLocalId buildingPersistentLocalId, CancellationToken cancellationToken)
        {
            var page = await _streamStore.ReadStreamBackwards(
                new StreamId(new BuildingStreamId(buildingPersistentLocalId)),
                StreamVersion.End,
                maxCount: 1,
                prefetchJsonData: false,
                cancellationToken);

            return page.Status == PageReadStatus.Success;
        }
    }
}
