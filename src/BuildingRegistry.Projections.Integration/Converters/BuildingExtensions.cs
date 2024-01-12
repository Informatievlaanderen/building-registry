namespace BuildingRegistry.Projections.Integration.Converters
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw;
    using BuildingRegistry.Building;

    public static class BuildingStatusExtensions
    {
        public static string Map(this BuildingStatus status)
        {
            if (status == BuildingStatus.Planned)
            {
                return GebouwStatus.Gepland.ToString();
            }

            if (status == BuildingStatus.UnderConstruction)
            {
                return GebouwStatus.InAanbouw.ToString();
            }

            if (status == BuildingStatus.NotRealized)
            {
                return GebouwStatus.NietGerealiseerd.ToString();
            }

            if (status == BuildingStatus.Realized)
            {
                return GebouwStatus.Gerealiseerd.ToString();
            }

            if (status == BuildingStatus.Retired)
            {
                return GebouwStatus.Gehistoreerd.ToString();
            }

            throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }
    }

    public static class BuildingGeometryMethodExtensions
    {
        public static string Map(this BuildingGeometryMethod geometryMethod)
        {
            if (geometryMethod == BuildingGeometryMethod.Outlined)
            {
                return GeometrieMethode.Ingeschetst.ToString();
            }

            if (geometryMethod == BuildingGeometryMethod.MeasuredByGrb)
            {
                return GeometrieMethode.IngemetenGRB.ToString();
            }

            throw new ArgumentOutOfRangeException(nameof(geometryMethod), geometryMethod, null);
        }
    }
}
