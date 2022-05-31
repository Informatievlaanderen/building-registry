namespace BuildingRegistry.Tests.Legacy.WhenImportingCrabBuildingStatus
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using BuildingRegistry.Legacy.Commands.Crab;
    using BuildingRegistry.Legacy.Crab;
    using BuildingRegistry.Legacy.Events.Crab;

    public static class ImportBuildingStatusFromCrabExtensions
    {
        public static BuildingStatusWasImportedFromCrab ToLegacyEvent(this ImportBuildingStatusFromCrab command)
        {
            return new BuildingStatusWasImportedFromCrab(
                command.BuildingStatusId,
                command.TerrainObjectId,
                command.BuildingStatus,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportBuildingStatusFromCrab WithStatus(this ImportBuildingStatusFromCrab command, CrabBuildingStatus crabBuildingStatus)
        {
            return new ImportBuildingStatusFromCrab(
                command.BuildingStatusId,
                command.TerrainObjectId,
                crabBuildingStatus,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportBuildingStatusFromCrab WithCrabModification(this ImportBuildingStatusFromCrab command, CrabModification? crabModification)
        {
            return new ImportBuildingStatusFromCrab(
                command.BuildingStatusId,
                command.TerrainObjectId,
                command.BuildingStatus,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                crabModification,
                command.Organisation);
        }

        public static ImportBuildingStatusFromCrab WithLifetime(this ImportBuildingStatusFromCrab command, CrabLifetime crabLifetime)
        {
            return new ImportBuildingStatusFromCrab(
                command.BuildingStatusId,
                command.TerrainObjectId,
                command.BuildingStatus,
                crabLifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportBuildingStatusFromCrab WithBuildingStatusId(this ImportBuildingStatusFromCrab command, CrabBuildingStatusId buildingStatusId)
        {
            return new ImportBuildingStatusFromCrab(
                buildingStatusId,
                command.TerrainObjectId,
                command.BuildingStatus,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }
    }
}
