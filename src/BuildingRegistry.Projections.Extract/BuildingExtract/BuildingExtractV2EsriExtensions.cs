namespace BuildingRegistry.Projections.Extract.BuildingExtract
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;

    public static class BuildingExtractV2EsriExtensions
    {
        public static async Task FindAndUpdateBuildingExtract(
            this ExtractContext context,
            int persistentLocalId,
            Action<BuildingExtractItemV2Esri> updateFunc,
            CancellationToken ct)
        {
            var building = await context
                .BuildingExtractV2Esri
                .FindAsync(persistentLocalId, cancellationToken: ct);

            updateFunc(building);
        }
    }
}
