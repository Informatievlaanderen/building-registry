namespace BuildingRegistry.Api.Oslo.Converters
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw;
    using Legacy;

    public static class BuildingStatusExtensions
    {
        public static BuildingRegistry.Building.BuildingStatus MapToV2(this GebouwStatus status)
        {
            switch (status)
            {
                case GebouwStatus.Gepland:
                    return BuildingRegistry.Building.BuildingStatus.Planned;

                case GebouwStatus.InAanbouw:
                    return BuildingRegistry.Building.BuildingStatus.UnderConstruction;

                case GebouwStatus.Gerealiseerd:
                    return BuildingRegistry.Building.BuildingStatus.Realized;

                case GebouwStatus.Gehistoreerd:
                    return BuildingRegistry.Building.BuildingStatus.Retired;

                case GebouwStatus.NietGerealiseerd:
                    return BuildingRegistry.Building.BuildingStatus.NotRealized;

                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }

        public static GebouwStatus Map(this BuildingRegistry.Building.BuildingStatus status)
        {
            if (status == BuildingRegistry.Building.BuildingStatus.Planned)
            {
                return GebouwStatus.Gepland;
            }
            if (status == BuildingRegistry.Building.BuildingStatus.UnderConstruction)
            {
                return GebouwStatus.InAanbouw;
            }
            if (status == BuildingRegistry.Building.BuildingStatus.NotRealized)
            {
                return GebouwStatus.NietGerealiseerd;
            }
            if (status == BuildingRegistry.Building.BuildingStatus.Realized)
            {
                return GebouwStatus.Gerealiseerd;
            }
            if (status == BuildingRegistry.Building.BuildingStatus.Retired)
            {
                return GebouwStatus.Gehistoreerd;
            }

            throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }
    }

    public static class BuildingGeometryMethodExtensions
    {
        public static GeometrieMethode Map(this BuildingGeometryMethod method)
        {
            switch (method)
            {
                case BuildingGeometryMethod.Outlined:
                    return GeometrieMethode.Ingeschetst;

                case BuildingGeometryMethod.MeasuredByGrb:
                    return GeometrieMethode.IngemetenGRB;

                default:
                    throw new ArgumentOutOfRangeException(nameof(method), method, null);
            }
        }

        public static GeometrieMethode Map(this BuildingRegistry.Building.BuildingGeometryMethod geometryMethod)
        {
            if (geometryMethod == BuildingRegistry.Building.BuildingGeometryMethod.Outlined)
            {
                return GeometrieMethode.Ingeschetst;
            }
            if (geometryMethod == BuildingRegistry.Building.BuildingGeometryMethod.MeasuredByGrb)
            {
                return GeometrieMethode.IngemetenGRB;
            }
            throw new ArgumentOutOfRangeException(nameof(geometryMethod), geometryMethod, null);
        }
    }
}
