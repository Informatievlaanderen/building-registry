namespace BuildingRegistry.Api.Oslo.Converters
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid;
    using BuildingRegistry.Building;

    public static class BuildingUnitStatusExtensions
    {
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

    public static class BuildingUnitFunctionExtensions
    {
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
