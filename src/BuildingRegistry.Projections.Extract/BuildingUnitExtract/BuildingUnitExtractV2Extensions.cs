namespace BuildingRegistry.Projections.Extract.BuildingUnitExtract
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;

    public static class BuildingUnitExtractV2Extensions
    {
        public static async Task FindAndUpdateBuildingUnitExtract(
            this ExtractContext context,
            int buildingUnitPersistentLocalId,
            Action<BuildingUnitExtractItemV2> updateFunc,
            CancellationToken ct)
        {
            var buildingUnit = await context
                .BuildingUnitExtractV2
                .FindAsync(buildingUnitPersistentLocalId, cancellationToken: ct);

            updateFunc(buildingUnit);
        }
    }
}
