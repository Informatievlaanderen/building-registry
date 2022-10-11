namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators
{
    using System.Linq;
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
            var endsWith = Pattern.EndsWith(new BuildingStreamId(buildingPersistentLocalId).ToString());
            var page = await _streamStore.ListStreams(endsWith, 1, cancellationToken: cancellationToken);
            return page?.StreamIds.Any() ?? false;
        }
    }
}
