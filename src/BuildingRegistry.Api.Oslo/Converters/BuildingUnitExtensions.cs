namespace BuildingRegistry.Api.Oslo.Converters
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid;
    using BuildingRegistry.Building;

    public static class BuildingUnitStatusExtensions
    {
        public static GebouweenheidStatus ConvertFromBuildingUnitStatus(this Legacy.BuildingUnitStatus status)
        {
            if (status == Legacy.BuildingUnitStatus.NotRealized)
            {
                return GebouweenheidStatus.NietGerealiseerd;
            }

            if (status == Legacy.BuildingUnitStatus.Planned)
            {
                return GebouweenheidStatus.Gepland;
            }

            if (status == Legacy.BuildingUnitStatus.Realized)
            {
                return GebouweenheidStatus.Gerealiseerd;
            }

            if (status == Legacy.BuildingUnitStatus.Retired)
            {
                return GebouweenheidStatus.Gehistoreerd;
            }

            throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }

        public static GebouweenheidStatus Map(this Legacy.BuildingUnitStatus status)
        {
            if (BuildingUnitStatus.Planned == status)
            {
                return GebouweenheidStatus.Gepland;
            }

            if (BuildingUnitStatus.NotRealized == status)
            {
                return GebouweenheidStatus.NietGerealiseerd;
            }

            if (BuildingUnitStatus.Realized == status)
            {
                return GebouweenheidStatus.Gerealiseerd;
            }

            if (BuildingUnitStatus.Retired == status)
            {
                return GebouweenheidStatus.Gehistoreerd;
            }

            throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }

        public static GebouweenheidStatus Map(this BuildingUnitStatus status)
        {
            if (BuildingUnitStatus.Planned == status)
            {
                return GebouweenheidStatus.Gepland;
            }

            if (BuildingUnitStatus.NotRealized == status)
            {
                return GebouweenheidStatus.NietGerealiseerd;
            }

            if (BuildingUnitStatus.Realized == status)
            {
                return GebouweenheidStatus.Gerealiseerd;
            }

            if (BuildingUnitStatus.Retired == status)
            {
                return GebouweenheidStatus.Gehistoreerd;
            }

            throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }

        public static Legacy.BuildingUnitStatus ConvertFromGebouweenheidStatus(this GebouweenheidStatus status)
        {
            if (status == GebouweenheidStatus.NietGerealiseerd)
            {
                return Legacy.BuildingUnitStatus.NotRealized;
            }

            if (status == GebouweenheidStatus.Gepland)
            {
                return Legacy.BuildingUnitStatus.Planned;
            }

            if (status == GebouweenheidStatus.Gerealiseerd)
            {
                return Legacy.BuildingUnitStatus.Realized;
            }

            if (status == GebouweenheidStatus.Gehistoreerd)
            {
                return Legacy.BuildingUnitStatus.Retired;
            }

            throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }

        public static BuildingUnitStatus Map(this GebouweenheidStatus status)
        {
            switch (status)
            {
                case GebouweenheidStatus.Gepland: return BuildingUnitStatus.Planned;
                case GebouweenheidStatus.Gerealiseerd: return BuildingUnitStatus.Realized;
                case GebouweenheidStatus.NietGerealiseerd: return BuildingUnitStatus.NotRealized;
                case GebouweenheidStatus.Gehistoreerd: return BuildingUnitStatus.Retired;
                default: throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }
    }

    public static class BuildingUnitPositionGeometryMethodExtensions
    {
        public static PositieGeometrieMethode ConvertFromBuildingUnitGeometryMethod(this Legacy.BuildingUnitPositionGeometryMethod method)
        {
            if (method == Legacy.BuildingUnitPositionGeometryMethod.DerivedFromObject)
            {
                return PositieGeometrieMethode.AfgeleidVanObject;
            }

            if (method == Legacy.BuildingUnitPositionGeometryMethod.AppointedByAdministrator)
            {
                return PositieGeometrieMethode.AangeduidDoorBeheerder;
            }

            throw new ArgumentOutOfRangeException(nameof(method), method, null);
        }
    }

    public static class BuildingUnitFunctionExtensions
    {
        public static GebouweenheidFunctie ConvertFromBuildingUnitFunction(this Legacy.BuildingUnitFunction function)
        {
            if (function == Legacy.BuildingUnitFunction.Unknown)
            {
                return GebouweenheidFunctie.NietGekend;
            }

            if (function == Legacy.BuildingUnitFunction.Common)
            {
                return GebouweenheidFunctie.GemeenschappelijkDeel;
            }

            throw new ArgumentOutOfRangeException(nameof(function), function, null);
        }
    }
}
