namespace BuildingRegistry.Migrator.Building.Projections
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using BuildingRegistry.Infrastructure;
    using Microsoft.EntityFrameworkCore;

    public class MigratorProjectionContext : RunnerDbContext<MigratorProjectionContext>
    {
        public MigratorProjectionContext()
        { }

        public MigratorProjectionContext(DbContextOptions<MigratorProjectionContext> options)
            : base(options)
        { }


        public override string ProjectionStateSchema => Schema.MigrateBuilding;
    }

    public class MigratorProjectionContextFactory : RunnerDbContextMigrationFactory<MigratorProjectionContext>
    {
        public MigratorProjectionContextFactory()
            : this("Events")
        { }

        public MigratorProjectionContextFactory(string connectionStringName)
            : base(connectionStringName, new MigrationHistoryConfiguration
            {
                Schema = Schema.MigrateBuilding,
                Table = MigrationTables.MigratorProjection
            })
        { }

        protected override MigratorProjectionContext CreateContext(DbContextOptions<MigratorProjectionContext> migrationContextOptions) => new MigratorProjectionContext(migrationContextOptions);

        public MigratorProjectionContext Create(DbContextOptions<MigratorProjectionContext> options) => CreateContext(options);
    }
}
