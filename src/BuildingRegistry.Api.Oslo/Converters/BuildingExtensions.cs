namespace BuildingRegistry.Api.Oslo.Converters
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw;
    using BuildingRegistry.Building;

    public static class BuildingStatusExtensions
    {
        public static BuildingStatus MapToV2(this GebouwStatus status)
        {
            switch (status)
            {
                case GebouwStatus.Gepland:
                    return BuildingStatus.Planned;

                case GebouwStatus.InAanbouw:
                    return BuildingStatus.UnderConstruction;

                case GebouwStatus.Gerealiseerd:
                    return BuildingStatus.Realized;

                case GebouwStatus.Gehistoreerd:
                    return BuildingStatus.Retired;

                case GebouwStatus.NietGerealiseerd:
                    return BuildingStatus.NotRealized;

                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }

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
}
