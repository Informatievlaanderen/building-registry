namespace BuildingRegistry.Tests.WhenImportingCrabHouseNumberPosition
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using Building.Commands.Crab;
    using Building.Events.Crab;
    using ValueObjects;
    using ValueObjects.Crab;

    public static class ImportHouseNumberPositionFromCrabExtensions
    {
        public static AddressHouseNumberPositionWasImportedFromCrab ToLegacyEvent(this ImportHouseNumberPositionFromCrab command)
        {
            return new AddressHouseNumberPositionWasImportedFromCrab(
                command.TerrainObjectId,
                command.TerrainObjectHouseNumberId,
                command.AddressPositionId,
                command.HouseNumberId,
                command.AddressPosition,
                command.AddressPositionOrigin,
                command.AddressNature,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportHouseNumberPositionFromCrab WithTerrainObjectHouseNumber(this ImportHouseNumberPositionFromCrab command, CrabTerrainObjectHouseNumberId terrainObjectHouseNumberId)
        {
            return new ImportHouseNumberPositionFromCrab(
                command.TerrainObjectId,
                terrainObjectHouseNumberId,
                command.AddressPositionId,
                command.HouseNumberId,
                command.AddressPosition,
                command.AddressNature,
                command.AddressPositionOrigin,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportHouseNumberPositionFromCrab WithTerrainObjectId(this ImportHouseNumberPositionFromCrab command, CrabTerrainObjectId terrainObjectId)
        {
            return new ImportHouseNumberPositionFromCrab(
                terrainObjectId,
                command.TerrainObjectHouseNumberId,
                command.AddressPositionId,
                command.HouseNumberId,
                command.AddressPosition,
                command.AddressNature,
                command.AddressPositionOrigin,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportHouseNumberPositionFromCrab WithPosition(this ImportHouseNumberPositionFromCrab command, WkbGeometry geometry)
        {
            return new ImportHouseNumberPositionFromCrab(
                command.TerrainObjectId,
                command.TerrainObjectHouseNumberId,
                command.AddressPositionId,
                command.HouseNumberId,
                geometry,
                command.AddressNature,
                command.AddressPositionOrigin,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportHouseNumberPositionFromCrab WithPositionOrigin(this ImportHouseNumberPositionFromCrab command, CrabAddressPositionOrigin origin)
        {
            return new ImportHouseNumberPositionFromCrab(
                command.TerrainObjectId,
                command.TerrainObjectHouseNumberId,
                command.AddressPositionId,
                command.HouseNumberId,
                command.AddressPosition,
                command.AddressNature,
                origin,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportHouseNumberPositionFromCrab WithModification(this ImportHouseNumberPositionFromCrab command, CrabModification? modification)
        {
            return new ImportHouseNumberPositionFromCrab(
                command.TerrainObjectId,
                command.TerrainObjectHouseNumberId,
                command.AddressPositionId,
                command.HouseNumberId,
                command.AddressPosition,
                command.AddressNature,
                command.AddressPositionOrigin,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                modification,
                command.Organisation);
        }

        public static ImportHouseNumberPositionFromCrab WithLifetime(this ImportHouseNumberPositionFromCrab command, CrabLifetime lifetime)
        {
            return new ImportHouseNumberPositionFromCrab(
                command.TerrainObjectId,
                command.TerrainObjectHouseNumberId,
                command.AddressPositionId,
                command.HouseNumberId,
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
