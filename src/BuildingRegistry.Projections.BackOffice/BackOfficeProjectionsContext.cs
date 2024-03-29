﻿namespace BuildingRegistry.Projections.BackOffice
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.SqlServer;
    using BuildingRegistry.Infrastructure;
    using Microsoft.EntityFrameworkCore;

    public class BackOfficeProjectionsContext : RunnerDbContext<BackOfficeProjectionsContext>
    {
        public override string ProjectionStateSchema => Schema.BackOfficeProjections;

        // This needs to be here to please EF
        public BackOfficeProjectionsContext()
        { }

        // This needs to be DbContextOptions<T> for Autofac!
        public BackOfficeProjectionsContext(DbContextOptions<BackOfficeProjectionsContext> options) : base(options)
        { }
    }

    public sealed class BackOfficeProjectionsContextMigrationFactory : SqlServerRunnerDbContextMigrationFactory<BackOfficeProjectionsContext>
    {
        public BackOfficeProjectionsContextMigrationFactory() : base("BackOfficeProjectionsAdmin", HistoryConfiguration)
        { }

        private static MigrationHistoryConfiguration HistoryConfiguration =>
            new MigrationHistoryConfiguration
            {
                Schema = Schema.BackOfficeProjections,
                Table = MigrationTables.BackOfficeProjections
            };

        protected override BackOfficeProjectionsContext CreateContext(DbContextOptions<BackOfficeProjectionsContext> migrationContextOptions)
            => new BackOfficeProjectionsContext(migrationContextOptions);
    }
}
