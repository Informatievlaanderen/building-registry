namespace BuildingRegistry.Api.Legacy.BuildingUnit
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid;
    using BuildingRegistry.Legacy;

    public static class BuildingUnitStatusExtensions
    {
        public static GebouweenheidStatus ConvertFromBuildingUnitStatus(this BuildingUnitStatus status)
        {
            if (status == BuildingUnitStatus.NotRealized)
            {
                return GebouweenheidStatus.NietGerealiseerd;
            }

            if (status == BuildingUnitStatus.Planned)
            {
                return GebouweenheidStatus.Gepland;
            }

            if (status == BuildingUnitStatus.Realized)
            {
                return GebouweenheidStatus.Gerealiseerd;
            }

            if (status == BuildingUnitStatus.Retired)
            {
                return GebouweenheidStatus.Gehistoreerd;
            }

            throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }

        public static BuildingUnitStatus ConvertFromGebouweenheidStatus(this GebouweenheidStatus status)
        {
            if (status == GebouweenheidStatus.NietGerealiseerd)
            {
                return BuildingUnitStatus.NotRealized;
            }

            if (status == GebouweenheidStatus.Gepland)
            {
                return BuildingUnitStatus.Planned;
            }

            if (status == GebouweenheidStatus.Gerealiseerd)
            {
                return BuildingUnitStatus.Realized;
            }

            if (status == GebouweenheidStatus.Gehistoreerd)
            {
                return BuildingUnitStatus.Retired;
            }

            throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }

        public static GebouweenheidStatus Map(this BuildingRegistry.Building.BuildingUnitStatus status)
        {
            if (BuildingRegistry.Building.BuildingUnitStatus.Planned == status)
            {
                return GebouweenheidStatus.Gepland;
            }

            if (BuildingRegistry.Building.BuildingUnitStatus.NotRealized == status)
            {
                return GebouweenheidStatus.NietGerealiseerd;
            }

            if (BuildingRegistry.Building.BuildingUnitStatus.Realized == status)
            {
                return GebouweenheidStatus.Gerealiseerd;
            }

            if (BuildingRegistry.Building.BuildingUnitStatus.Retired == status)
            {
                return GebouweenheidStatus.Gehistoreerd;
            }

            throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }
    }

    public static class BuildingUnitPositionGeometryMethodExtensions
    {
        public static PositieGeometrieMethode ConvertFromBuildingUnitGeometryMethod(this BuildingUnitPositionGeometryMethod method)
        {
            if (method == BuildingUnitPositionGeometryMethod.DerivedFromObject)
            {
                return PositieGeometrieMethode.AfgeleidVanObject;
            }

            if (method == BuildingUnitPositionGeometryMethod.AppointedByAdministrator)
            {
                return PositieGeometrieMethode.AangeduidDoorBeheerder;
            }

            throw new ArgumentOutOfRangeException(nameof(method), method, null);
        }

        public static PositieGeometrieMethode Map(this BuildingRegistry.Building.BuildingUnitPositionGeometryMethod method)
        {
            if (method == BuildingRegistry.Building.BuildingUnitPositionGeometryMethod.DerivedFromObject)
            {
                return PositieGeometrieMethode.AfgeleidVanObject;
            }

            if (method == BuildingRegistry.Building.BuildingUnitPositionGeometryMethod.AppointedByAdministrator)
            {
                return PositieGeometrieMethode.AangeduidDoorBeheerder;
            }

            throw new ArgumentOutOfRangeException(nameof(method), method, null);
        }
    }

    public static class BuildingUnitFunctionExtensions
    {
        public static GebouweenheidFunctie? ConvertFromBuildingUnitFunction(this BuildingUnitFunction? function)
        {
            if (function == null)
            {
                return null;
            }

            if (function == BuildingUnitFunction.Unknown)
            {
                return GebouweenheidFunctie.NietGekend;
            }

            if (function == BuildingUnitFunction.Common)
            {
                return GebouweenheidFunctie.GemeenschappelijkDeel;
            }

            throw new ArgumentOutOfRangeException(nameof(function), function, null);
        }

        public static BuildingRegistry.Building.BuildingUnitFunction Map(this GebouweenheidFunctie functie)
        {
            return functie switch
            {
                GebouweenheidFunctie.NietGekend => BuildingRegistry.Building.BuildingUnitFunction.Unknown,
                GebouweenheidFunctie.GemeenschappelijkDeel => BuildingRegistry.Building.BuildingUnitFunction.Common,
                _ => throw new ArgumentOutOfRangeException(nameof(functie), functie, null)
            };
        }

        public static GebouweenheidFunctie? Map(this BuildingRegistry.Building.BuildingUnitFunction function)
        {
            if (BuildingRegistry.Building.BuildingUnitFunction.Common == function)
            {
                return GebouweenheidFunctie.GemeenschappelijkDeel;
            }

            if (BuildingRegistry.Building.BuildingUnitFunction.Unknown == function)
            {
                return GebouweenheidFunctie.NietGekend;
            }

            throw new ArgumentOutOfRangeException(nameof(function), function, null);
        }
    }
}
