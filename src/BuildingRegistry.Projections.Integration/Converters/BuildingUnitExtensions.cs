namespace BuildingRegistry.Projections.Integration.Converters
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid;
    using BuildingRegistry.Building;

    public static class BuildingUnitStatusExtensions
    {
        public static string Map(this BuildingUnitStatus status)
        {
            if (BuildingUnitStatus.Planned == status)
            {
                return GebouweenheidStatus.Gepland.ToString();
            }

            if (BuildingUnitStatus.NotRealized == status)
            {
                return GebouweenheidStatus.NietGerealiseerd.ToString();
            }

            if (BuildingUnitStatus.Realized == status)
            {
                return GebouweenheidStatus.Gerealiseerd.ToString();
            }

            if (BuildingUnitStatus.Retired == status)
            {
                return GebouweenheidStatus.Gehistoreerd.ToString();
            }

            throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }
    }

    public static class BuildingUnitPositionGeometryMethodExtensions
    {
        public static string Map(this BuildingUnitPositionGeometryMethod geometryMethod)
        {
            if (BuildingUnitPositionGeometryMethod.DerivedFromObject == geometryMethod)
            {
                return PositieGeometrieMethode.AfgeleidVanObject.ToString();
            }

            if (BuildingUnitPositionGeometryMethod.AppointedByAdministrator == geometryMethod)
            {
                return PositieGeometrieMethode.AangeduidDoorBeheerder.ToString();
            }

            throw new ArgumentOutOfRangeException(nameof(geometryMethod), geometryMethod, null);
        }
    }

    public static class BuildingUnitFunctionExtensions
    {
        public static string Map(this BuildingUnitFunction function)
        {
            if (function == BuildingUnitFunction.Unknown)
            {
                return GebouweenheidFunctie.NietGekend.ToString();
            }

            if (function == BuildingUnitFunction.Common)
            {
                return GebouweenheidFunctie.GemeenschappelijkDeel.ToString();
            }

            throw new ArgumentOutOfRangeException(nameof(function), function, null);
        }
    }
}
