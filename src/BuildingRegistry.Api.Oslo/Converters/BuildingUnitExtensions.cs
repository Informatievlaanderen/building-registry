namespace BuildingRegistry.Api.Oslo.Converters
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid;
    using BuildingRegistry.Building;

    public static class BuildingUnitStatusExtensions
    {
        public static GebouweenheidStatus ConvertFromBuildingUnitStatus(this BuildingRegistry.Legacy.BuildingUnitStatus status)
        {
            if (status == BuildingRegistry.Legacy.BuildingUnitStatus.NotRealized)
            {
                return GebouweenheidStatus.NietGerealiseerd;
            }

            if (status == BuildingRegistry.Legacy.BuildingUnitStatus.Planned)
            {
                return GebouweenheidStatus.Gepland;
            }

            if (status == BuildingRegistry.Legacy.BuildingUnitStatus.Realized)
            {
                return GebouweenheidStatus.Gerealiseerd;
            }

            if (status == BuildingRegistry.Legacy.BuildingUnitStatus.Retired)
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
        public static PositieGeometrieMethode ConvertFromBuildingUnitGeometryMethod(this BuildingRegistry.Legacy.BuildingUnitPositionGeometryMethod method)
        {
            if (method == BuildingRegistry.Legacy.BuildingUnitPositionGeometryMethod.DerivedFromObject)
            {
                return PositieGeometrieMethode.AfgeleidVanObject;
            }

            if (method == BuildingRegistry.Legacy.BuildingUnitPositionGeometryMethod.AppointedByAdministrator)
            {
                return PositieGeometrieMethode.AangeduidDoorBeheerder;
            }

            throw new ArgumentOutOfRangeException(nameof(method), method, null);
        }
    }

    public static class BuildingUnitFunctionExtensions
    {
        public static GebouweenheidFunctie? ConvertFromBuildingUnitFunction(this BuildingRegistry.Legacy.BuildingUnitFunction? function)
        {
            if (function == null)
            {
                return null;
            }

            if (function == BuildingRegistry.Legacy.BuildingUnitFunction.Unknown)
            {
                return GebouweenheidFunctie.NietGekend;
            }

            if (function == BuildingRegistry.Legacy.BuildingUnitFunction.Common)
            {
                return GebouweenheidFunctie.GemeenschappelijkDeel;
            }

            throw new ArgumentOutOfRangeException(nameof(function), function, null);
        }

        public static BuildingUnitFunction Map(this GebouweenheidFunctie functie)
        {
            switch (functie)
            {
                case GebouweenheidFunctie.NietGekend: return BuildingUnitFunction.Unknown;
                case GebouweenheidFunctie.GemeenschappelijkDeel: return BuildingUnitFunction.Common;
                default: throw new ArgumentOutOfRangeException(nameof(functie), functie, null);
            }
        }
    }
}
