namespace BuildingRegistry.Tests.WhenReaddressingHouseNumber
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using Building.Commands.Crab;
    using Building.Events.Crab;
    using ValueObjects;

    public static class ImportReaddressingHouseNumberFromCrabExtensions
    {
        public static HouseNumberWasReaddressedFromCrab ToLegacyEvent(
            this ImportReaddressingHouseNumberFromCrab command)
        {
            return new HouseNumberWasReaddressedFromCrab(
                command.TerrainObjectId,
                command.ReaddressingId,
                command.BeginDate,
                command.OldTerrainObjectHouseNumberId,
                command.OldAddressNature,
                command.OldHouseNumberId,
                command.NewTerrainObjectHouseNumberId,
                command.NewAddressNature,
                command.NewHouseNumberId);
        }

        public static ImportReaddressingHouseNumberFromCrab WithBeginDate(this ImportReaddressingHouseNumberFromCrab command, ReaddressingBeginDate beginDate)
        {
            return new ImportReaddressingHouseNumberFromCrab(
                command.TerrainObjectId,
                command.ReaddressingId,
                beginDate,
                command.OldHouseNumberId,
                command.OldAddressNature,
                command.OldTerrainObjectHouseNumberId,
                command.NewHouseNumberId,
                command.NewAddressNature,
                command.NewTerrainObjectHouseNumberId);
        }

        public static ImportReaddressingHouseNumberFromCrab WithOldTerrainObjectHouseNumberId(this ImportReaddressingHouseNumberFromCrab command, CrabTerrainObjectHouseNumberId oldTerrainObjectHouseNumberId)
        {
            return new ImportReaddressingHouseNumberFromCrab(
                command.TerrainObjectId,
                command.ReaddressingId,
                command.BeginDate,
                command.OldHouseNumberId,
                command.OldAddressNature,
                oldTerrainObjectHouseNumberId,
                command.NewHouseNumberId,
                command.NewAddressNature,
                command.NewTerrainObjectHouseNumberId);
        }

        public static ImportReaddressingHouseNumberFromCrab WithOldHouseNumberId(this ImportReaddressingHouseNumberFromCrab command, CrabHouseNumberId oldHouseNumberId)
        {
            return new ImportReaddressingHouseNumberFromCrab(
                command.TerrainObjectId,
                command.ReaddressingId,
                command.BeginDate,
                oldHouseNumberId,
                command.OldAddressNature,
                command.OldTerrainObjectHouseNumberId,
                command.NewHouseNumberId,
                command.NewAddressNature,
                command.NewTerrainObjectHouseNumberId);
        }

        public static ImportReaddressingHouseNumberFromCrab WithNewTerrainObjectHouseNumberId(this ImportReaddressingHouseNumberFromCrab command, CrabTerrainObjectHouseNumberId newTerrainObjectHouseNumberId)
        {
            return new ImportReaddressingHouseNumberFromCrab(
                command.TerrainObjectId,
                command.ReaddressingId,
                command.BeginDate,
                command.OldHouseNumberId,
                command.OldAddressNature,
                command.OldTerrainObjectHouseNumberId,
                command.NewHouseNumberId,
                command.NewAddressNature,
                newTerrainObjectHouseNumberId);
        }

        public static ImportReaddressingHouseNumberFromCrab WithNewHouseNumberId(this ImportReaddressingHouseNumberFromCrab command, CrabHouseNumberId newHouseNumberId)
        {
            return new ImportReaddressingHouseNumberFromCrab(
                command.TerrainObjectId,
                command.ReaddressingId,
                command.BeginDate,
                command.OldHouseNumberId,
                command.OldAddressNature,
                command.OldTerrainObjectHouseNumberId,
                newHouseNumberId,
                command.NewAddressNature,
                command.NewTerrainObjectHouseNumberId);
        }

        public static ImportReaddressingHouseNumberFromCrab WithTerrainObjectId(this ImportReaddressingHouseNumberFromCrab command, CrabTerrainObjectId terrainObjectId)
        {
            return new ImportReaddressingHouseNumberFromCrab(
                terrainObjectId,
                command.ReaddressingId,
                command.BeginDate,
                command.OldHouseNumberId,
                command.OldAddressNature,
                command.OldTerrainObjectHouseNumberId,
                command.NewHouseNumberId,
                command.NewAddressNature,
                command.NewTerrainObjectHouseNumberId);
        }
    }
}
