namespace BuildingRegistry.Tests.Legacy.WhenImportingCrabSubaddress
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using BuildingRegistry.Legacy.Commands.Crab;
    using BuildingRegistry.Legacy.Events.Crab;

    public static class ImportSubaddressFromCrabExtensions
    {
        public static AddressSubaddressWasImportedFromCrab ToLegacyEvent(this ImportSubaddressFromCrab command)
        {
            return new AddressSubaddressWasImportedFromCrab(
                command.TerrainObjectId,
                command.TerrainObjectHouseNumberId,
                command.SubaddressId,
                command.HouseNumberId,
                command.BoxNumber,
                command.BoxNumberType,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportSubaddressFromCrab WithModification(this ImportSubaddressFromCrab command,
            CrabModification? modification)
        {
            return new ImportSubaddressFromCrab(
                command.TerrainObjectId,
                command.TerrainObjectHouseNumberId,
                command.SubaddressId,
                command.HouseNumberId,
                command.BoxNumber,
                command.BoxNumberType,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                modification,
                command.Organisation);
        }

        public static ImportSubaddressFromCrab WithLifetime(this ImportSubaddressFromCrab command, CrabLifetime lifetime)
        {
            return new ImportSubaddressFromCrab(
                command.TerrainObjectId,
                command.TerrainObjectHouseNumberId,
                command.SubaddressId,
                command.HouseNumberId,
                command.BoxNumber,
                command.BoxNumberType,
                lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportSubaddressFromCrab WithTimestamp(this ImportSubaddressFromCrab command, CrabTimestamp timestamp)
        {
            return new ImportSubaddressFromCrab(
                command.TerrainObjectId,
                command.TerrainObjectHouseNumberId,
                command.SubaddressId,
                command.HouseNumberId,
                command.BoxNumber,
                command.BoxNumberType,
                command.Lifetime,
                timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportSubaddressFromCrab WithSubaddressId(this ImportSubaddressFromCrab command, CrabSubaddressId subaddressId)
        {
            return new ImportSubaddressFromCrab(
                command.TerrainObjectId,
                command.TerrainObjectHouseNumberId,
                subaddressId,
                command.HouseNumberId,
                command.BoxNumber,
                command.BoxNumberType,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportSubaddressFromCrab WithTerrainObjectHouseNumberId(this ImportSubaddressFromCrab command, CrabTerrainObjectHouseNumberId terrainObjectHouseNumberId)
        {
            return new ImportSubaddressFromCrab(
                command.TerrainObjectId,
                terrainObjectHouseNumberId,
                command.SubaddressId,
                command.HouseNumberId,
                command.BoxNumber,
                command.BoxNumberType,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportSubaddressFromCrab WithHouseNumberId(
            this ImportSubaddressFromCrab command,
            CrabHouseNumberId houseNumberId)
        {
            return new ImportSubaddressFromCrab(
                command.TerrainObjectId,
                command.TerrainObjectHouseNumberId,
                command.SubaddressId,
                houseNumberId,
                command.BoxNumber,
                command.BoxNumberType,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }
    }
}
