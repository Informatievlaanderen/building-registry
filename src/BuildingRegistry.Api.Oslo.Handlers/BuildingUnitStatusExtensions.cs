namespace BuildingRegistry.Api.Oslo.Handlers
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid;
    using Legacy;

    public static class BuildingUnitStatusExtensions
    {
        public static GebouweenheidStatus MapBuildingUnitStatus(this BuildingUnitStatus status)
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
    }
}
