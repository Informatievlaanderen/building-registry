namespace BuildingRegistry.Api.Legacy.Abstractions.Converters
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw;
    using BuildingRegistry.Building;
    using Legacy = BuildingRegistry.Legacy;

    public static class BuildingStatusExtensions
    {
        public static GebouwStatus ConvertFromBuildingStatus(this Legacy.BuildingStatus status)
        {
            switch (status)
            {
                case Legacy.BuildingStatus.Planned:
                    return GebouwStatus.Gepland;

                case Legacy.BuildingStatus.UnderConstruction:
                    return GebouwStatus.InAanbouw;

                case Legacy.BuildingStatus.Realized:
                    return GebouwStatus.Gerealiseerd;

                case Legacy.BuildingStatus.Retired:
                    return GebouwStatus.Gehistoreerd;

                case Legacy.BuildingStatus.NotRealized:
                    return GebouwStatus.NietGerealiseerd;

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

        public static Legacy.BuildingStatus ConvertFromGebouwStatus(this GebouwStatus status)
        {
            switch (status)
            {
                case GebouwStatus.Gepland:
                    return Legacy.BuildingStatus.Planned;

                case GebouwStatus.InAanbouw:
                    return Legacy.BuildingStatus.UnderConstruction;

                case GebouwStatus.Gerealiseerd:
                    return Legacy.BuildingStatus.Realized;

                case GebouwStatus.Gehistoreerd:
                    return Legacy.BuildingStatus.Retired;

                case GebouwStatus.NietGerealiseerd:
                    return Legacy.BuildingStatus.NotRealized;

                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }

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
    }

    public static class BuildingGeometryMethodExtensions
    {
        public static GeometrieMethode ConvertFromBuildingGeometryMethod(this Legacy.BuildingGeometryMethod method)
        {
            switch (method)
            {
                case Legacy.BuildingGeometryMethod.Outlined:
                    return GeometrieMethode.Ingeschetst;

                case Legacy.BuildingGeometryMethod.MeasuredByGrb:
                    return GeometrieMethode.IngemetenGRB;

                default:
                    throw new ArgumentOutOfRangeException(nameof(method), method, null);
            }
        }

        public static GeometrieMethode Map(this BuildingGeometryMethod method)
        {
            if (method == BuildingGeometryMethod.Outlined)
            {
                return GeometrieMethode.Ingeschetst;
            }

            if (method == BuildingGeometryMethod.MeasuredByGrb)
            {
                return GeometrieMethode.IngemetenGRB;
            }

            throw new ArgumentOutOfRangeException(nameof(method), method, null);
        }
    }
}
