namespace BuildingRegistry.Tests.Legacy.WhenImportingCrabSubaddressStatus
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using BuildingRegistry.Legacy.Commands.Crab;
    using BuildingRegistry.Legacy.Events.Crab;

    public static class ImportSubaddressStatusFromCrabExtensions
    {
        public static AddressSubaddressStatusWasImportedFromCrab ToLegacyEvent(this ImportSubaddressStatusFromCrab command)
        {
            return new AddressSubaddressStatusWasImportedFromCrab(
                command.TerrainObjectId,
                command.TerrainObjectHouseNumberId,
                command.SubaddressStatusId,
                command.SubaddressId,
                command.SubaddressStatus,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportSubaddressStatusFromCrab WithTerrainObjectHouseNumberId(this ImportSubaddressStatusFromCrab command, CrabTerrainObjectHouseNumberId terrainObjectHouseNumberId)
        {
            return new ImportSubaddressStatusFromCrab(
                command.TerrainObjectId,
                terrainObjectHouseNumberId,
                command.SubaddressStatusId,
                command.SubaddressId,
                command.SubaddressStatus,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportSubaddressStatusFromCrab WithSubaddressId(this ImportSubaddressStatusFromCrab command, CrabSubaddressId subaddressId)
        {
            return new ImportSubaddressStatusFromCrab(
                command.TerrainObjectId,
                command.TerrainObjectHouseNumberId,
                command.SubaddressStatusId,
                subaddressId,
                command.SubaddressStatus,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportSubaddressStatusFromCrab WithStatus(this ImportSubaddressStatusFromCrab command, CrabAddressStatus status)
        {
            return new ImportSubaddressStatusFromCrab(
                command.TerrainObjectId,
                command.TerrainObjectHouseNumberId,
                command.SubaddressStatusId,
                command.SubaddressId,
                status,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportSubaddressStatusFromCrab WithModification(this ImportSubaddressStatusFromCrab command, CrabModification? modification)
        {
            return new ImportSubaddressStatusFromCrab(
                command.TerrainObjectId,
                command.TerrainObjectHouseNumberId,
                command.SubaddressStatusId,
                command.SubaddressId,
                command.SubaddressStatus,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                modification,
                command.Organisation);
        }

        public static ImportSubaddressStatusFromCrab WithLifetime(this ImportSubaddressStatusFromCrab command, CrabLifetime lifetime)
        {
            return new ImportSubaddressStatusFromCrab(
                command.TerrainObjectId,
                command.TerrainObjectHouseNumberId,
                command.SubaddressStatusId,
                command.SubaddressId,
                command.SubaddressStatus,
                lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportSubaddressStatusFromCrab WithTimestamp(this ImportSubaddressStatusFromCrab command, CrabTimestamp timestamp)
        {
            return new ImportSubaddressStatusFromCrab(
                command.TerrainObjectId,
                command.TerrainObjectHouseNumberId,
                command.SubaddressStatusId,
                command.SubaddressId,
                command.SubaddressStatus,
                command.Lifetime,
                timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }
    }
}
