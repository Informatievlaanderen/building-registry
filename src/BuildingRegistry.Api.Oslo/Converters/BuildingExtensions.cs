namespace BuildingRegistry.Api.Oslo.Converters
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw;
    using BuildingRegistry.Legacy;

    public static class BuildingStatusExtensions
    {
        public static GebouwStatus ConvertFromBuildingStatus(this BuildingStatus status)
        {
            switch (status)
            {
                case BuildingStatus.Planned:
                    return GebouwStatus.Gepland;

                case BuildingStatus.UnderConstruction:
                    return GebouwStatus.InAanbouw;

                case BuildingStatus.Realized:
                    return GebouwStatus.Gerealiseerd;

                case BuildingStatus.Retired:
                    return GebouwStatus.Gehistoreerd;

                case BuildingStatus.NotRealized:
                    return GebouwStatus.NietGerealiseerd;

                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }

        public static BuildingStatus ConvertFromGebouwStatus(this GebouwStatus status)
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
    }

    public static class BuildingGeometryMethodExtensions
    {
        public static GeometrieMethode ConvertFromBuildingGeometryMethod(this BuildingGeometryMethod method)
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
    }
}
