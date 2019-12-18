namespace BuildingRegistry.Tests.WhenImportingCrabBuildingGeometry
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using Building.Commands.Crab;
    using Building.Events.Crab;
    using ValueObjects;
    using ValueObjects.Crab;

    public static class ImportBuildingGeometryFromCrabExtensions
    {
        public static BuildingGeometryWasImportedFromCrab ToLegacyEvent(this ImportBuildingGeometryFromCrab command)
        {
            return new BuildingGeometryWasImportedFromCrab(
                command.BuildingGeometryId,
                command.TerrainObjectId,
                command.BuildingGeometry,
                command.BuildingGeometryMethod,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportBuildingGeometryFromCrab WithGeometryMethod(this ImportBuildingGeometryFromCrab command, CrabBuildingGeometryMethod crabBuildingGeometryMethod)
        {
            return new ImportBuildingGeometryFromCrab(
                command.BuildingGeometryId,
                command.TerrainObjectId,
                command.BuildingGeometry,
                crabBuildingGeometryMethod,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportBuildingGeometryFromCrab WithGeometry(this ImportBuildingGeometryFromCrab command, WkbGeometry crabBuildingGeometry)
        {
            return new ImportBuildingGeometryFromCrab(
                command.BuildingGeometryId,
                command.TerrainObjectId,
                crabBuildingGeometry,
                command.BuildingGeometryMethod,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportBuildingGeometryFromCrab WithCrabModification(this ImportBuildingGeometryFromCrab command, CrabModification? crabModification)
        {
            return new ImportBuildingGeometryFromCrab(
                command.BuildingGeometryId,
                command.TerrainObjectId,
                command.BuildingGeometry,
                command.BuildingGeometryMethod,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                crabModification,
                command.Organisation);
        }

        public static ImportBuildingGeometryFromCrab WithLifetime(this ImportBuildingGeometryFromCrab command, CrabLifetime crabLifetime)
        {
            return new ImportBuildingGeometryFromCrab(
                command.BuildingGeometryId,
                command.TerrainObjectId,
                command.BuildingGeometry,
                command.BuildingGeometryMethod,
                crabLifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportBuildingGeometryFromCrab WithBuildingGeometryId(this ImportBuildingGeometryFromCrab command, CrabBuildingGeometryId buildingGeometryId)
        {
            return new ImportBuildingGeometryFromCrab(
                buildingGeometryId,
                command.TerrainObjectId,
                command.BuildingGeometry,
                command.BuildingGeometryMethod,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportBuildingGeometryFromCrab WithTerrainObjectId(this ImportBuildingGeometryFromCrab command, CrabTerrainObjectId terrainObjectId)
        {
            return new ImportBuildingGeometryFromCrab(
                command.BuildingGeometryId,
                terrainObjectId,
                command.BuildingGeometry,
                command.BuildingGeometryMethod,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportBuildingGeometryFromCrab WithTimestamp(this ImportBuildingGeometryFromCrab command, CrabTimestamp timestamp)
        {
            return new ImportBuildingGeometryFromCrab(
                command.BuildingGeometryId,
                command.TerrainObjectId,
                command.BuildingGeometry,
                command.BuildingGeometryMethod,
                command.Lifetime,
                timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }
    }
}
