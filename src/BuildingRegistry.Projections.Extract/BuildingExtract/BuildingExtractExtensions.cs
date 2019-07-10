namespace BuildingRegistry.Projections.Extract.BuildingExtract
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;

    public static class BuildingExtractExtensions
    {
        public static async Task FindAndUpdateBuildingExtract(
            this ExtractContext context,
            Guid buildingId,
            Action<BuildingExtractItem> updateFunc,
            CancellationToken ct)
        {
            var building = await context
                .BuildingExtract
                .FindAsync(buildingId, cancellationToken: ct);

            if (building == null) //building can be removed and events keep pouring in (building unit clean up events, persistent local id, etc)
                return;

            updateFunc(building);
        }

        private static ProjectionItemNotFoundException<BuildingExtractProjections> DatabaseItemNotFound(Guid buildingId)
            => new ProjectionItemNotFoundException<BuildingExtractProjections>(buildingId.ToString("D"));
    }
}
