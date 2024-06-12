namespace BuildingRegistry.Consumer.Read.Parcel
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.SqlServer.MigrationExtensions;
    using Building;
    using BuildingRegistry.Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.Extensions.Configuration;
    using NetTopologySuite.Geometries;
    using ParcelWithCount;

    public class ConsumerParcelContext : RunnerDbContext<ConsumerParcelContext>, IParcels
    {
        public DbSet<ParcelConsumerItem> ParcelConsumerItemsWithCount { get; set; }
        public DbSet<ParcelAddressItem> ParcelAddressItemsWithCount { get; set; }

        // This needs to be here to please EF
        public ConsumerParcelContext()
        { }

        // This needs to be DbContextOptions<T> for Autofac!
        public ConsumerParcelContext(DbContextOptions<ConsumerParcelContext> options)
            : base(options)
        { }

        public async Task AddIdempotentParcelAddress(Guid parcelId, int addressPersistentLocalId, CancellationToken ct)
        {
            var parcelAddressItem =
                await ParcelAddressItemsWithCount.FindAsync([parcelId, addressPersistentLocalId], cancellationToken: ct);

            if (parcelAddressItem is null)
            {
                ParcelAddressItemsWithCount.Add(new ParcelAddressItem(parcelId, addressPersistentLocalId));
            }
        }

        public async Task RemoveIdempotentParcelAddress(Guid parcelId, int addressPersistentLocalId, CancellationToken ct)
        {
            var parcelAddressItem =
                await ParcelAddressItemsWithCount.FindAsync([parcelId, addressPersistentLocalId], cancellationToken: ct);

            if (parcelAddressItem is not null)
            {
                ParcelAddressItemsWithCount.Remove(parcelAddressItem);
            }
        }

        public override string ProjectionStateSchema => Schema.ConsumerReadParcel;

        public async Task<IEnumerable<ParcelData>> GetUnderlyingParcelsUnderBoundingBox(Geometry buildingGeometry)
        {
            var boundingBox = buildingGeometry.Factory.ToGeometry(buildingGeometry.EnvelopeInternal);

            return await ParcelConsumerItemsWithCount
                .Where(parcel => boundingBox.Intersects(parcel.Geometry))
                .Select(x => new ParcelData(
                    x.ParcelId,
                    x.CaPaKey,
                    x.Geometry,
                    x.Status,
                    ParcelAddressItemsWithCount
                        .Where(y => y.ParcelId == x.ParcelId)
                        .Select(y => new AddressPersistentLocalId(y.AddressPersistentLocalId))
                        .ToList()))
                .ToListAsync();
        }
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
                    sqlServerOptions.UseNetTopologySuite();
                })
                .UseExtendedSqlServerMigrations();

            return new ConsumerParcelContext(builder.Options);
        }
    }
}
