namespace BuildingRegistry.Api.Legacy.Converters
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw;
    using System;
    using ValueObjects;

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
