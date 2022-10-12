namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators
{
    using System.Threading;
    using System.Threading.Tasks;
    using BuildingRegistry.Building;
    using SqlStreamStore;

    public sealed class BuildingExistsValidator
    {
        private readonly IStreamStore _streamStore;

        public BuildingExistsValidator(IStreamStore streamStore)
        {
            _streamStore = streamStore;
        }

        public async Task<bool> Exists(BuildingPersistentLocalId buildingPersistentLocalId, CancellationToken cancellationToken)
        {
            var streamMetadata = await _streamStore.GetStreamMetadata(new BuildingStreamId(buildingPersistentLocalId), cancellationToken);
            return streamMetadata.MetadataStreamVersion != -1;
        }
    }
}
