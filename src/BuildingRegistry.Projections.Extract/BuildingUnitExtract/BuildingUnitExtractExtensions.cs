namespace BuildingRegistry.Projections.Extract.BuildingUnitExtract
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;

    public static class BuildingUnitExtractExtensions
    {
        public static async Task<BuildingUnitExtractItem> FindAndUpdateBuildingUnitExtract(
            this ExtractContext context,
            Guid buildingUnitId,
            Action<BuildingUnitExtractItem> updateFunc,
            CancellationToken ct)
        {
            var buildingUnit = await context
                .BuildingUnitExtract
                .FindAsync(buildingUnitId, cancellationToken: ct);

            if (buildingUnit == null)
                throw DatabaseItemNotFound(buildingUnitId);

            updateFunc(buildingUnit);

            return buildingUnit;
        }

        private static ProjectionItemNotFoundException<BuildingUnitExtractProjections> DatabaseItemNotFound(Guid buildingUnitId)
            => new ProjectionItemNotFoundException<BuildingUnitExtractProjections>(buildingUnitId.ToString("D"));
    }
}
