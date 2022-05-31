namespace BuildingRegistry.Api.Legacy.Converters
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid;
    using System;
    using BuildingRegistry.Legacy;

    public static class BuildingUnitStatusExtensions
    {
        public static GebouweenheidStatus ConvertFromBuildingUnitStatus(this BuildingUnitStatus status)
        {
            if (status == BuildingUnitStatus.NotRealized)
                return GebouweenheidStatus.NietGerealiseerd;

            if (status == BuildingUnitStatus.Planned)
                return GebouweenheidStatus.Gepland;

            if (status == BuildingUnitStatus.Realized)
                return GebouweenheidStatus.Gerealiseerd;

            if (status == BuildingUnitStatus.Retired)
                return GebouweenheidStatus.Gehistoreerd;

            throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }

        public static BuildingUnitStatus ConvertFromGebouweenheidStatus(this GebouweenheidStatus status)
        {
            if (status == GebouweenheidStatus.NietGerealiseerd)
                return BuildingUnitStatus.NotRealized;

            if (status == GebouweenheidStatus.Gepland)
                return BuildingUnitStatus.Planned;

            if (status == GebouweenheidStatus.Gerealiseerd)
                return BuildingUnitStatus.Realized;

            if (status == GebouweenheidStatus.Gehistoreerd)
                return BuildingUnitStatus.Retired;

            throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }
    }

    public static class BuildingUnitPositionGeometryMethodExtensions
    {
        public static PositieGeometrieMethode ConvertFromBuildingUnitGeometryMethod(this BuildingUnitPositionGeometryMethod method)
        {
            if (method == BuildingUnitPositionGeometryMethod.DerivedFromObject)
                return PositieGeometrieMethode.AfgeleidVanObject;

            if (method == BuildingUnitPositionGeometryMethod.AppointedByAdministrator)
                return PositieGeometrieMethode.AangeduidDoorBeheerder;

            throw new ArgumentOutOfRangeException(nameof(method), method, null);
        }
    }

    public static class BuildingUnitFunctionExtensions
    {
        public static GebouweenheidFunctie ConvertFromBuildingUnitFunction(this BuildingUnitFunction function)
        {
            if (function == BuildingUnitFunction.Unknown)
                return GebouweenheidFunctie.NietGekend;

            if (function == BuildingUnitFunction.Common)
                return GebouweenheidFunctie.GemeenschappelijkDeel;

            throw new ArgumentOutOfRangeException(nameof(function), function, null);
        }
    }
}
