namespace BuildingRegistry.Projections.Extract.BuildingExtract
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;

    public static class BuildingExtractV2Extensions
    {
        public static async Task FindAndUpdateBuildingExtract(
            this ExtractContext context,
            int persistentLocalId,
            Action<BuildingExtractItemV2> updateFunc,
            CancellationToken ct)
        {
            var building = await context
                .BuildingExtractV2
                .FindAsync(persistentLocalId, cancellationToken: ct);

            updateFunc(building);
        }
    }
}
