namespace BuildingRegistry.Infrastructure
{
    public static class Schema
    {
        public const string Default = "BuildingRegistry";
        public const string Import = "BuildingRegistryImport";
        public const string Legacy = "BuildingRegistryLegacy";
        public const string Extract = "BuildingRegistryExtract";
        public const string Wms = "wms";
        public const string Wfs = "wfs";
        public const string ConsumerAddress = "BuildingRegistryConsumerAddress";
        public const string ConsumerReadParcel = "BuildingRegistryConsumerReadParcel";
        public const string MigrateBuilding = "BuildingRegistryMigration";
        public const string Sequence = "BuildingRegistrySequence";
        public const string BackOffice = "BuildingRegistryBackOffice";
        public const string BackOfficeProjections = "BuildingRegistryBackOfficeProjections";
        public const string Producer = "BuildingRegistryProducer";
        public const string ProducerSnapshotOslo = "BuildingRegistryProducerSnapshotOslo";
        public const string Integration = "integration_building";
        public const string Tools = "BuildingRegistryTools";
    }

    public static class MigrationTables
    {
        public const string Legacy = "__EFMigrationsHistoryLegacy";
        public const string Wms = "__EFMigrationsHistoryWmsBuilding";
        public const string Wfs = "__EFMigrationsHistoryWfsBuilding";
        public const string Extract = "__EFMigrationsHistoryExtract";
        public const string RedisDataMigration = "__EFMigrationsHistoryDataMigration";
        public const string Sequence = "__EFMigrationsHistorySequence";
        public const string ConsumerAddress = "__EFMigrationsHistoryConsumerAddress";
        public const string ConsumerReadParcel = "__EFMigrationsHistoryConsumerReadParcel";
        public const string BackOffice = "__EFMigrationsHistoryBackOffice";
        public const string BackOfficeProjections = "__EFMigrationsHistoryBackOfficeProjections";
        public const string MigratorProjection = "__EFMigrationsHistoryMigrationProjection";
        public const string Producer = "__EFMigrationsHistoryProducer";
        public const string ProducerSnapshotOslo = "__EFMigrationsHistoryProducerSnapshotOslo";
        public const string Integration = "__EFMigrationsHistory";
    }
}
