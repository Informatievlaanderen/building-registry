namespace BuildingRegistry.Migrator.Building.Projections
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using BuildingRegistry.Infrastructure;
    using Microsoft.EntityFrameworkCore;

    public class MigratorConsumerContext : RunnerDbContext<MigratorConsumerContext>
    {
        public MigratorConsumerContext()
        { }

        public MigratorConsumerContext(DbContextOptions<MigratorConsumerContext> options)
            : base(options)
        { }


        public override string ProjectionStateSchema => Schema.MigrateBuilding;
    }

    public class MigratorConsumerContextFactory : RunnerDbContextMigrationFactory<MigratorConsumerContext>
    {
        public MigratorConsumerContextFactory()
            : this("Events")
        { }

        public MigratorConsumerContextFactory(string connectionStringName)
            : base(connectionStringName, new MigrationHistoryConfiguration
            {
                Schema = Schema.MigrateBuilding,
                Table = MigrationTables.MigratorConsumer
            })
        { }

        protected override MigratorConsumerContext CreateContext(DbContextOptions<MigratorConsumerContext> migrationContextOptions) => new MigratorConsumerContext(migrationContextOptions);

        public MigratorConsumerContext Create(DbContextOptions<MigratorConsumerContext> options) => CreateContext(options);
    }
}
