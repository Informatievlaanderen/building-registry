namespace BuildingRegistry.Building
{
    using Be.Vlaanderen.Basisregisters.Crab;

    public partial class Building
    {
        public void FixGrar1359(
            CrabTerrainObjectId terrainObjectId,
            CrabTerrainObjectHouseNumberId terrainObjectHouseNumberId,
            CrabSubaddressId subaddressId,
            CrabHouseNumberId houseNumberId,
            CrabLifetime lifetime,
            CrabTimestamp timestamp,
            CrabModification? modification)
        {
            if (IsRemoved)
                return;

            ImportSubaddressFromCrab(
                terrainObjectId,
                terrainObjectHouseNumberId,
                houseNumberId,
                subaddressId,
                lifetime,
                timestamp,
                modification);
        }
    }
}
