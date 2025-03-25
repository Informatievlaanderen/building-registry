namespace BuildingRegistry.Producer.Ldes
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Building;

    public static class BuildingStatusExtensions
    {
        public static GebouwStatus Map(this BuildingStatus status)
        {
            if (status == BuildingStatus.Planned)
            {
                return GebouwStatus.Gepland;
            }
            if (status == BuildingStatus.UnderConstruction)
            {
                return GebouwStatus.InAanbouw;
            }
            if (status == BuildingStatus.NotRealized)
            {
                return GebouwStatus.NietGerealiseerd;
            }
            if (status == BuildingStatus.Realized)
            {
                return GebouwStatus.Gerealiseerd;
            }
            if (status == BuildingStatus.Retired)
            {
                return GebouwStatus.Gehistoreerd;
            }

            throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }
    }

    public static class BuildingGeometryMethodExtensions
    {
        public static GeometrieMethode Map(this BuildingGeometryMethod geometryMethod)
        {
            if (geometryMethod == BuildingGeometryMethod.Outlined)
            {
                return GeometrieMethode.Ingeschetst;
            }
            if (geometryMethod == BuildingGeometryMethod.MeasuredByGrb)
            {
                return GeometrieMethode.IngemetenGRB;
            }
            throw new ArgumentOutOfRangeException(nameof(geometryMethod), geometryMethod, null);
        }
    }

    public static class BuildingDetailExtensions
    {
        public static async Task FindAndUpdateBuilding(
            this ProducerContext context,
            int persistentLocalId,
            Action<BuildingDetail> updateFunc,
            CancellationToken ct)
        {
            var building = await context
                .Buildings
                .FindAsync(persistentLocalId, cancellationToken: ct);

            if (building is null)
            {
                throw new ProjectionItemNotFoundException<ProducerProjections>(persistentLocalId.ToString());
            }

            updateFunc(building);
        }
    }
}
