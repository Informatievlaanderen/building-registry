
namespace BuildingRegistry.Tests.Legacy.WhenImportingCrabTerrainObjectHouseNumber
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using BuildingRegistry.Legacy.Commands.Crab;
    using BuildingRegistry.Legacy.Events.Crab;

    public static class ImportTerrainObjectHouseNumberFromCrabExtensions
    {
        public static TerrainObjectHouseNumberWasImportedFromCrab ToLegacyEvent(this ImportTerrainObjectHouseNumberFromCrab command)
        {
            return new TerrainObjectHouseNumberWasImportedFromCrab(
                command.TerrainObjectHouseNumberId,
                command.TerrainObjectId,
                command.HouseNumberId,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportTerrainObjectHouseNumberFromCrab WithModification(this ImportTerrainObjectHouseNumberFromCrab command,
            CrabModification? modification)
        {
            return new ImportTerrainObjectHouseNumberFromCrab(
                command.TerrainObjectHouseNumberId,
                command.TerrainObjectId,
                command.HouseNumberId,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                modification,
                command.Organisation);
        }

        public static ImportTerrainObjectHouseNumberFromCrab WithLifetime(this ImportTerrainObjectHouseNumberFromCrab command, CrabLifetime lifetime)
        {
            return new ImportTerrainObjectHouseNumberFromCrab(
                command.TerrainObjectHouseNumberId,
                command.TerrainObjectId,
                command.HouseNumberId,
                lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportTerrainObjectHouseNumberFromCrab WithTimestamp(this ImportTerrainObjectHouseNumberFromCrab command, CrabTimestamp timestamp)
        {
            return new ImportTerrainObjectHouseNumberFromCrab(
                command.TerrainObjectHouseNumberId,
                command.TerrainObjectId,
                command.HouseNumberId,
                command.Lifetime,
                timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportTerrainObjectHouseNumberFromCrab WithTerrainObjectId(
            this ImportTerrainObjectHouseNumberFromCrab command,
            CrabTerrainObjectId terrainObjectId)
        {
            return new ImportTerrainObjectHouseNumberFromCrab(
                command.TerrainObjectHouseNumberId,
                terrainObjectId,
                command.HouseNumberId,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportTerrainObjectHouseNumberFromCrab WithTerrainObjectHouseNumberId(
            this ImportTerrainObjectHouseNumberFromCrab command,
            CrabTerrainObjectHouseNumberId terrainObjectHouseNumberId)
        {
            return new ImportTerrainObjectHouseNumberFromCrab(
                terrainObjectHouseNumberId,
                command.TerrainObjectId,
                command.HouseNumberId,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportTerrainObjectHouseNumberFromCrab WithHouseNumberId(
            this ImportTerrainObjectHouseNumberFromCrab command,
            CrabHouseNumberId houseNumberId)
        {
            return new ImportTerrainObjectHouseNumberFromCrab(
                command.TerrainObjectHouseNumberId,
                command.TerrainObjectId,
                houseNumberId,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

    }
}
