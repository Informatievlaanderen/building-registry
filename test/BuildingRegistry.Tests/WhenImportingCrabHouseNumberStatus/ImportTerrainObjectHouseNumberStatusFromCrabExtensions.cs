namespace BuildingRegistry.Tests.WhenImportingCrabHouseNumberStatus
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using Building.Commands.Crab;
    using Building.Events.Crab;
    using ValueObjects.Crab;

    public static class ImportHouseNumberStatusFromCrabExtensions
    {
        public static AddressHouseNumberStatusWasImportedFromCrab ToLegacyEvent(this ImportHouseNumberStatusFromCrab command)
        {
            return new AddressHouseNumberStatusWasImportedFromCrab(
                command.TerrainObjectId,
                command.TerrainObjectHouseNumberId,
                command.HouseNumberStatusId,
                command.HouseNumberId,
                command.AddressStatus,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }


        public static ImportHouseNumberStatusFromCrab WithTerrainObjectHouseNumberId(this ImportHouseNumberStatusFromCrab command, CrabTerrainObjectHouseNumberId terrainObjectHouseNumberId)
        {
            return new ImportHouseNumberStatusFromCrab(
                command.TerrainObjectId,
                terrainObjectHouseNumberId,
                command.HouseNumberStatusId,
                command.HouseNumberId,
                command.AddressStatus,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportHouseNumberStatusFromCrab WithStatus(this ImportHouseNumberStatusFromCrab command, CrabAddressStatus status)
        {
            return new ImportHouseNumberStatusFromCrab(
                command.TerrainObjectId,
                command.TerrainObjectHouseNumberId,
                command.HouseNumberStatusId,
                command.HouseNumberId,
                status,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportHouseNumberStatusFromCrab WithHouseNumberId(this ImportHouseNumberStatusFromCrab command, CrabHouseNumberId houseNumberId)
        {
            return new ImportHouseNumberStatusFromCrab(
                command.TerrainObjectId,
                command.TerrainObjectHouseNumberId,
                command.HouseNumberStatusId,
                houseNumberId,
                command.AddressStatus,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportHouseNumberStatusFromCrab WithModification(this ImportHouseNumberStatusFromCrab command, CrabModification? modification)
        {
            return new ImportHouseNumberStatusFromCrab(
                command.TerrainObjectId,
                command.TerrainObjectHouseNumberId,
                command.HouseNumberStatusId,
                command.HouseNumberId,
                command.AddressStatus,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                modification,
                command.Organisation);
        }

        public static ImportHouseNumberStatusFromCrab WithLifetime(this ImportHouseNumberStatusFromCrab command, CrabLifetime lifetime)
        {
            return new ImportHouseNumberStatusFromCrab(
                command.TerrainObjectId,
                command.TerrainObjectHouseNumberId,
                command.HouseNumberStatusId,
                command.HouseNumberId,
                command.AddressStatus,
                lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportHouseNumberStatusFromCrab WithTimestamp(this ImportHouseNumberStatusFromCrab command, CrabTimestamp timestamp)
        {
            return new ImportHouseNumberStatusFromCrab(
                command.TerrainObjectId,
                command.TerrainObjectHouseNumberId,
                command.HouseNumberStatusId,
                command.HouseNumberId,
                command.AddressStatus,
                command.Lifetime,
                timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }
    }
}
