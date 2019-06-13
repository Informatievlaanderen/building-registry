namespace BuildingRegistry.Tests.WhenReaddressingSubaddress
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using Building.Commands.Crab;
    using Building.Events.Crab;
    using ValueObjects;

    public static class ImportReaddressingSubaddressFromCrabExtensions
    {
        public static SubaddressWasReaddressedFromCrab ToLegacyEvent(
            this ImportReaddressingSubaddressFromCrab command)
        {
            return new SubaddressWasReaddressedFromCrab(
                command.TerrainObjectId,
                command.ReaddressingId,
                command.BeginDate,
                command.OldTerrainObjectHouseNumberId,
                command.OldAddressNature,
                command.OldSubaddressId,
                command.NewTerrainObjectHouseNumberId,
                command.NewAddressNature,
                command.NewSubaddressId);
        }

        public static ImportReaddressingSubaddressFromCrab WithBeginDate(this ImportReaddressingSubaddressFromCrab command, ReaddressingBeginDate beginDate)
        {
            return new ImportReaddressingSubaddressFromCrab(
                command.TerrainObjectId,
                command.ReaddressingId,
                beginDate,
                command.OldSubaddressId,
                command.OldAddressNature,
                command.OldTerrainObjectHouseNumberId,
                command.NewSubaddressId,
                command.NewAddressNature,
                command.NewTerrainObjectHouseNumberId);
        }

        public static ImportReaddressingSubaddressFromCrab WithOldTerrainObjectHouseNumberId(this ImportReaddressingSubaddressFromCrab command, CrabTerrainObjectHouseNumberId oldTerrainObjectHouseNumberId)
        {
            return new ImportReaddressingSubaddressFromCrab(
                command.TerrainObjectId,
                command.ReaddressingId,
                command.BeginDate,
                command.OldSubaddressId,
                command.OldAddressNature,
                oldTerrainObjectHouseNumberId,
                command.NewSubaddressId,
                command.NewAddressNature,
                command.NewTerrainObjectHouseNumberId);
        }

        public static ImportReaddressingSubaddressFromCrab WithOldSubaddressId(this ImportReaddressingSubaddressFromCrab command, CrabSubaddressId oldSubaddressId)
        {
            return new ImportReaddressingSubaddressFromCrab(
                command.TerrainObjectId,
                command.ReaddressingId,
                command.BeginDate,
                oldSubaddressId,
                command.OldAddressNature,
                command.OldTerrainObjectHouseNumberId,
                command.NewSubaddressId,
                command.NewAddressNature,
                command.NewTerrainObjectHouseNumberId);
        }

        public static ImportReaddressingSubaddressFromCrab WithNewTerrainObjectHouseNumberId(this ImportReaddressingSubaddressFromCrab command, CrabTerrainObjectHouseNumberId newTerrainObjectHouseNumberId)
        {
            return new ImportReaddressingSubaddressFromCrab(
                command.TerrainObjectId,
                command.ReaddressingId,
                command.BeginDate,
                command.OldSubaddressId,
                command.OldAddressNature,
                command.OldTerrainObjectHouseNumberId,
                command.NewSubaddressId,
                command.NewAddressNature,
                newTerrainObjectHouseNumberId);
        }

        public static ImportReaddressingSubaddressFromCrab WithNewSubaddressId(this ImportReaddressingSubaddressFromCrab command, CrabSubaddressId newSubaddressId)
        {
            return new ImportReaddressingSubaddressFromCrab(
                command.TerrainObjectId,
                command.ReaddressingId,
                command.BeginDate,
                command.OldSubaddressId,
                command.OldAddressNature,
                command.OldTerrainObjectHouseNumberId,
                newSubaddressId,
                command.NewAddressNature,
                command.NewTerrainObjectHouseNumberId);
        }

        public static ImportReaddressingSubaddressFromCrab WithTerrainObjectId(this ImportReaddressingSubaddressFromCrab command, CrabTerrainObjectId terrainObjectId)
        {
            return new ImportReaddressingSubaddressFromCrab(
                terrainObjectId,
                command.ReaddressingId,
                command.BeginDate,
                command.OldSubaddressId,
                command.OldAddressNature,
                command.OldTerrainObjectHouseNumberId,
                command.NewSubaddressId,
                command.NewAddressNature,
                command.NewTerrainObjectHouseNumberId);
        }
    }
}
