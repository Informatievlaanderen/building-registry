namespace BuildingRegistry.Consumer.Read.Parcel
{
    using System;
    using System.IO;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.MigrationExtensions;
    using BuildingRegistry.Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.Extensions.Configuration;

    public class ConsumerParcelContext : RunnerDbContext<ConsumerParcelContext>
    {
        public DbSet<ParcelConsumerItem> ParcelConsumerItems { get; set; }

        // This needs to be here to please EF
        public ConsumerParcelContext()
        { }

        // This needs to be DbContextOptions<T> for Autofac!
        public ConsumerParcelContext(DbContextOptions<ConsumerParcelContext> options)
            : base(options)
        { }

        public override string ProjectionStateSchema => Schema.ConsumerReadParcel;
    }

    public class ConsumerContextFactory : IDesignTimeDbContextFactory<ConsumerParcelContext>
    {
        public ConsumerParcelContext CreateDbContext(string[] args)
        {
            const string migrationConnectionStringName = "ConsumerParcelAdmin";

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            var builder = new DbContextOptionsBuilder<ConsumerParcelContext>();

            var connectionString = configuration.GetConnectionString(migrationConnectionStringName);
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException($"Could not find a connection string with name '{migrationConnectionStringName}'");

            builder
                .UseSqlServer(connectionString, sqlServerOptions =>
                {
                    sqlServerOptions.EnableRetryOnFailure();
                    sqlServerOptions.MigrationsHistoryTable(MigrationTables.ConsumerReadParcel, Schema.ConsumerReadParcel);
                })
                .UseExtendedSqlServerMigrations();

            return new ConsumerParcelContext(builder.Options);
        }
    }
}
