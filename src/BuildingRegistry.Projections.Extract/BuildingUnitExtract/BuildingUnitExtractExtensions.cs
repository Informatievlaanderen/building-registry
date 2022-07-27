namespace BuildingRegistry.Projections.Extract.BuildingUnitExtract
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;

    public static class BuildingUnitExtractExtensions
    {
        public static async Task FindAndUpdateBuildingUnitExtract(
            this ExtractContext context,
            Guid buildingUnitId,
            Action<BuildingUnitExtractItem> updateFunc,
            CancellationToken ct)
        {
            var buildingUnit = await context
                .BuildingUnitExtract
                .FindAsync(buildingUnitId, cancellationToken: ct);

            if (buildingUnit == null) //after building unit is removed, events can keep pouring in
            {
                return;
            }

            updateFunc(buildingUnit);
        }
    }
}
