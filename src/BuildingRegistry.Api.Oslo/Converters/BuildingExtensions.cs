namespace BuildingRegistry.Api.Oslo.Converters
{
    using System;
    using BuildingRegistry.Building;

    public static class BuildingStatusExtensions
    {
        public static BuildingStatus MapToV2(this Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw.GebouwStatus status)
        {
            switch (status)
            {
                case Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw.GebouwStatus.Gepland:
                    return BuildingStatus.Planned;

                case Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw.GebouwStatus.InAanbouw:
                    return BuildingStatus.UnderConstruction;

                case Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw.GebouwStatus.Gerealiseerd:
                    return BuildingStatus.Realized;

                case Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw.GebouwStatus.Gehistoreerd:
                    return BuildingStatus.Retired;

                case Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw.GebouwStatus.NietGerealiseerd:
                    return BuildingStatus.NotRealized;

                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }

        public static Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw.GebouwStatus Map(this BuildingStatus status)
        {
            if (status == BuildingStatus.Planned)
            {
                return Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw.GebouwStatus.Gepland;
            }
            if (status == BuildingStatus.UnderConstruction)
            {
                return Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw.GebouwStatus.InAanbouw;
            }
            if (status == BuildingStatus.NotRealized)
            {
                return Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw.GebouwStatus.NietGerealiseerd;
            }
            if (status == BuildingStatus.Realized)
            {
                return Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw.GebouwStatus.Gerealiseerd;
            }
            if (status == BuildingStatus.Retired)
            {
                return Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw.GebouwStatus.Gehistoreerd;
            }

            throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }

        public static Be.Vlaanderen.Basisregisters.GrAr.Oslo.Gebouw.GebouwStatusValue MapOslo(this BuildingStatus status)
        {
            if (status == BuildingStatus.Planned)
            {
                return Be.Vlaanderen.Basisregisters.GrAr.Oslo.Gebouw.GebouwStatusValue.Gepland;
            }
            if (status == BuildingStatus.UnderConstruction)
            {
                return Be.Vlaanderen.Basisregisters.GrAr.Oslo.Gebouw.GebouwStatusValue.InAanbouw;
            }
            if (status == BuildingStatus.NotRealized)
            {
                return Be.Vlaanderen.Basisregisters.GrAr.Oslo.Gebouw.GebouwStatusValue.NietGerealiseerd;
            }
            if (status == BuildingStatus.Realized)
            {
                return Be.Vlaanderen.Basisregisters.GrAr.Oslo.Gebouw.GebouwStatusValue.Gerealiseerd;
            }
            if (status == BuildingStatus.Retired)
            {
                return Be.Vlaanderen.Basisregisters.GrAr.Oslo.Gebouw.GebouwStatusValue.Gehistoreerd;
            }

            throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }

        public static Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw.GebouwStatus ConvertFromBuildingStatus(this BuildingRegistry.Legacy.BuildingStatus status)
        {
            switch (status)
            {
                case BuildingRegistry.Legacy.BuildingStatus.Planned:
                    return Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw.GebouwStatus.Gepland;

                case BuildingRegistry.Legacy.BuildingStatus.UnderConstruction:
                    return Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw.GebouwStatus.InAanbouw;

                case BuildingRegistry.Legacy.BuildingStatus.Realized:
                    return Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw.GebouwStatus.Gerealiseerd;

                case BuildingRegistry.Legacy.BuildingStatus.Retired:
                    return Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw.GebouwStatus.Gehistoreerd;

                case BuildingRegistry.Legacy.BuildingStatus.NotRealized:
                    return Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw.GebouwStatus.NietGerealiseerd;

                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }
    }

    public static class BuildingGeometryMethodExtensions
    {
        public static Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw.GeometrieMethode ConvertFromBuildingGeometryMethod(this BuildingRegistry.Legacy.BuildingGeometryMethod method)
        {
            switch (method)
            {
                case BuildingRegistry.Legacy.BuildingGeometryMethod.Outlined:
                    return Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw.GeometrieMethode.Ingeschetst;

                case BuildingRegistry.Legacy.BuildingGeometryMethod.MeasuredByGrb:
                    return Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw.GeometrieMethode.IngemetenGRB;

                default:
                    throw new ArgumentOutOfRangeException(nameof(method), method, null);
            }
        }

        public static Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw.GeometrieMethode Map(this BuildingGeometryMethod geometryMethod)
        {
            if (geometryMethod == BuildingGeometryMethod.Outlined)
            {
                return Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw.GeometrieMethode.Ingeschetst;
            }
            if (geometryMethod == BuildingGeometryMethod.MeasuredByGrb)
            {
                return Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw.GeometrieMethode.IngemetenGRB;
            }
            throw new ArgumentOutOfRangeException(nameof(geometryMethod), geometryMethod, null);
        }

        public static Be.Vlaanderen.Basisregisters.GrAr.Oslo.Gebouw.GebouwGeometrieMethode MapOslo(this BuildingGeometryMethod geometryMethod)
        {
            if (geometryMethod == BuildingGeometryMethod.Outlined)
            {
                return Be.Vlaanderen.Basisregisters.GrAr.Oslo.Gebouw.GebouwGeometrieMethode.Ingeschetst;
            }
            if (geometryMethod == BuildingGeometryMethod.MeasuredByGrb)
            {
                return Be.Vlaanderen.Basisregisters.GrAr.Oslo.Gebouw.GebouwGeometrieMethode.IngemetenGRB;
            }
            throw new ArgumentOutOfRangeException(nameof(geometryMethod), geometryMethod, null);
        }
    }
}
