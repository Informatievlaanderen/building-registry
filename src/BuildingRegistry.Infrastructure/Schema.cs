namespace BuildingRegistry.Infrastructure
{
    public static class Schema
    {
        public const string Default = "BuildingRegistry";

        public const string Import = "BuildingRegistryImport";
        public const string Legacy = "BuildingRegistryLegacy";
        public const string Extract = "BuildingRegistryExtract";
        public const string Syndication = "BuildingRegistrySyndication";
        
        public const string Wms = "wms";
        public const string Wfs = "wfs";
        
        public const string ConsumerAddress = "ConsumerAddress";

        public const string Sequence = "BuildingRegistrySequence";
    }

    public static class MigrationTables
    {
        public const string Legacy = "__EFMigrationsHistoryLegacy";
        public const string Wms = "__EFMigrationsHistoryWmsBuilding";
        public const string Wfs = "__EFMigrationsHistoryWfsBuilding";
        public const string Extract = "__EFMigrationsHistoryExtract";
        public const string Syndication = "__EFMigrationsHistorySyndication";
        public const string RedisDataMigration = "__EFMigrationsHistoryDataMigration";
        public const string Sequence = "__EFMigrationsHistorySequence";
        public const string ConsumerAddress = "__EFMigrationsHistoryConsumerAddress";
    }
}
