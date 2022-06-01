namespace BuildingRegistry.Api.Oslo.Handlers
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw;
    using BuildingRegistry.Legacy;

    public abstract class BaseHandler
    {
        public static GebouwStatus MapBuildingStatus(BuildingStatus status)
        {
            switch (status)
            {
                case BuildingStatus.Planned:
                    return GebouwStatus.Gepland;

                case BuildingStatus.UnderConstruction:
                    return GebouwStatus.InAanbouw;

                case BuildingStatus.Realized:
                    return GebouwStatus.Gerealiseerd;

                case BuildingStatus.Retired:
                    return GebouwStatus.Gehistoreerd;

                case BuildingStatus.NotRealized:
                    return GebouwStatus.NietGerealiseerd;

                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }
    }
}
