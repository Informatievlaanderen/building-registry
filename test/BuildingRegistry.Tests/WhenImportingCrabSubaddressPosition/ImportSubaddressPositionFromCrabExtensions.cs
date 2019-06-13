namespace BuildingRegistry.Tests.WhenImportingCrabSubaddressPosition
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using Building.Commands.Crab;
    using Building.Events.Crab;
    using ValueObjects;
    using ValueObjects.Crab;

    public static class ImportSubaddressPositionFromCrabExtensions
    {
        public static AddressSubaddressPositionWasImportedFromCrab ToLegacyEvent(this ImportSubaddressPositionFromCrab command)
        {
            return new AddressSubaddressPositionWasImportedFromCrab(
                command.TerrainObjectId,
                command.TerrainObjectHouseNumberId,
                command.AddressPositionId,
                command.SubaddressId,
                command.AddressPosition,
                command.AddressPositionOrigin,
                command.AddressNature,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportSubaddressPositionFromCrab WithTerrainObjectHouseNumberId(this ImportSubaddressPositionFromCrab command, CrabTerrainObjectHouseNumberId terrainObjectHouseNumberId)
        {
            return new ImportSubaddressPositionFromCrab(
                command.TerrainObjectId,
                terrainObjectHouseNumberId,
                command.AddressPositionId,
                command.SubaddressId,
                command.AddressPosition,
                command.AddressNature,
                command.AddressPositionOrigin,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportSubaddressPositionFromCrab WithSubaddressId(this ImportSubaddressPositionFromCrab command, CrabSubaddressId subaddressId)
        {
            return new ImportSubaddressPositionFromCrab(
                command.TerrainObjectId,
                command.TerrainObjectHouseNumberId,
                command.AddressPositionId,
                subaddressId,
                command.AddressPosition,
                command.AddressNature,
                command.AddressPositionOrigin,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportSubaddressPositionFromCrab WithPosition(this ImportSubaddressPositionFromCrab command, WkbGeometry geometry)
        {
            return new ImportSubaddressPositionFromCrab(
                command.TerrainObjectId,
                command.TerrainObjectHouseNumberId,
                command.AddressPositionId,
                command.SubaddressId,
                geometry,
                command.AddressNature,
                command.AddressPositionOrigin,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportSubaddressPositionFromCrab WithPositionOrigin(this ImportSubaddressPositionFromCrab command, CrabAddressPositionOrigin origin)
        {
            return new ImportSubaddressPositionFromCrab(
                command.TerrainObjectId,
                command.TerrainObjectHouseNumberId,
                command.AddressPositionId,
                command.SubaddressId,
                command.AddressPosition,
                command.AddressNature,
                origin,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportSubaddressPositionFromCrab WithModification(this ImportSubaddressPositionFromCrab command, CrabModification? modification)
        {
            return new ImportSubaddressPositionFromCrab(
                command.TerrainObjectId,
                command.TerrainObjectHouseNumberId,
                command.AddressPositionId,
                command.SubaddressId,
                command.AddressPosition,
                command.AddressNature,
                command.AddressPositionOrigin,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                modification,
                command.Organisation);
        }

        public static ImportSubaddressPositionFromCrab WithLifetime(this ImportSubaddressPositionFromCrab command, CrabLifetime lifetime)
        {
            return new ImportSubaddressPositionFromCrab(
                command.TerrainObjectId,
                command.TerrainObjectHouseNumberId,
                command.AddressPositionId,
                command.SubaddressId,
                command.AddressPosition,
                command.AddressNature,
                command.AddressPositionOrigin,
                lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }
    }
}
