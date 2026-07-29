namespace BuildingRegistry.Api.Oslo.Converters
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using BuildingRegistry.Building;

    public static class BuildingUnitStatusExtensions
    {
        public static Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid.GebouweenheidStatus ConvertFromBuildingUnitStatus(this BuildingRegistry.Legacy.BuildingUnitStatus status)
        {
            if (status == BuildingRegistry.Legacy.BuildingUnitStatus.NotRealized)
            {
                return Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid.GebouweenheidStatus.NietGerealiseerd;
            }

            if (status == BuildingRegistry.Legacy.BuildingUnitStatus.Planned)
            {
                return Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid.GebouweenheidStatus.Gepland;
            }

            if (status == BuildingRegistry.Legacy.BuildingUnitStatus.Realized)
            {
                return Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid.GebouweenheidStatus.Gerealiseerd;
            }

            if (status == BuildingRegistry.Legacy.BuildingUnitStatus.Retired)
            {
                return Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid.GebouweenheidStatus.Gehistoreerd;
            }

            throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }

        public static Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid.GebouweenheidStatus Map(this BuildingUnitStatus status)
        {
            if (BuildingUnitStatus.Planned == status)
            {
                return Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid.GebouweenheidStatus.Gepland;
            }

            if (BuildingUnitStatus.NotRealized == status)
            {
                return Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid.GebouweenheidStatus.NietGerealiseerd;
            }

            if (BuildingUnitStatus.Realized == status)
            {
                return Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid.GebouweenheidStatus.Gerealiseerd;
            }

            if (BuildingUnitStatus.Retired == status)
            {
                return Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid.GebouweenheidStatus.Gehistoreerd;
            }

            throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }

        public static Be.Vlaanderen.Basisregisters.GrAr.Oslo.Gebouweenheid.GebouweenheidStatusValue MapOslo(this BuildingUnitStatus status)
        {
            if (BuildingUnitStatus.Planned == status)
            {
                return Be.Vlaanderen.Basisregisters.GrAr.Oslo.Gebouweenheid.GebouweenheidStatusValue.Gepland;
            }

            if (BuildingUnitStatus.NotRealized == status)
            {
                return Be.Vlaanderen.Basisregisters.GrAr.Oslo.Gebouweenheid.GebouweenheidStatusValue.NietGerealiseerd;
            }

            if (BuildingUnitStatus.Realized == status)
            {
                return Be.Vlaanderen.Basisregisters.GrAr.Oslo.Gebouweenheid.GebouweenheidStatusValue.Gerealiseerd;
            }

            if (BuildingUnitStatus.Retired == status)
            {
                return Be.Vlaanderen.Basisregisters.GrAr.Oslo.Gebouweenheid.GebouweenheidStatusValue.Gehistoreerd;
            }

            throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }

        public static BuildingUnitStatus Map(this Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid.GebouweenheidStatus status)
        {
            switch (status)
            {
                case Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid.GebouweenheidStatus.Gepland: return BuildingUnitStatus.Planned;
                case Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid.GebouweenheidStatus.Gerealiseerd: return BuildingUnitStatus.Realized;
                case Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid.GebouweenheidStatus.NietGerealiseerd: return BuildingUnitStatus.NotRealized;
                case Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid.GebouweenheidStatus.Gehistoreerd: return BuildingUnitStatus.Retired;
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
        public static Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid.GebouweenheidFunctie? ConvertFromBuildingUnitFunction(this BuildingRegistry.Legacy.BuildingUnitFunction? function)
        {
            if (function == null)
            {
                return null;
            }

            if (function == BuildingRegistry.Legacy.BuildingUnitFunction.Unknown)
            {
                return Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid.GebouweenheidFunctie.NietGekend;
            }

            if (function == BuildingRegistry.Legacy.BuildingUnitFunction.Common)
            {
                return Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid.GebouweenheidFunctie.GemeenschappelijkDeel;
            }

            throw new ArgumentOutOfRangeException(nameof(function), function, null);
        }

        public static BuildingUnitFunction Map(this Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid.GebouweenheidFunctie functie)
        {
            switch (functie)
            {
                case Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid.GebouweenheidFunctie.NietGekend: return BuildingUnitFunction.Unknown;
                case Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid.GebouweenheidFunctie.GemeenschappelijkDeel: return BuildingUnitFunction.Common;
                default: throw new ArgumentOutOfRangeException(nameof(functie), functie, null);
            }
        }
    }
}
