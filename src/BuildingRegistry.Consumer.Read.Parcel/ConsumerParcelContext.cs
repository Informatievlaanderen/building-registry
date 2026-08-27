namespace BuildingRegistry.Consumer.Read.Parcel
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
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
        public DbSet<ParcelConsumerItem> ParcelConsumerItemsWithCount => Set<ParcelConsumerItem>();
        public DbSet<ParcelAddressItem> ParcelAddressItemsWithCount => Set<ParcelAddressItem>();

        public DbSet<BuildingToInvalidate> BuildingsToInvalidate => Set<BuildingToInvalidate>();

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

        public async Task<IEnumerable<ParcelData>> GetUnderlyingParcelsUnderBoundingBox(Geometry buildingGeometry, int matchingSrid)
        {
            var boundingBox = buildingGeometry.Factory.ToGeometry(buildingGeometry.EnvelopeInternal);

            // Two near-identical queries rather than one with a conditional projection: EF has to translate
            // the column into the SQL predicate, so which column is compared cannot be chosen inside it.
            if (matchingSrid == SystemReferenceId.SridLambert2008)
            {
                return await ParcelConsumerItemsWithCount
                    .Where(parcel => boundingBox.Intersects(parcel.GeometryLambert2008))
                    .Select(x => new ParcelData(
                        x.ParcelId,
                        x.CaPaKey,
                        x.GeometryLambert2008!,
                        x.Status,
                        ParcelAddressItemsWithCount
                            .Where(y => y.ParcelId == x.ParcelId)
                            .Select(y => new AddressPersistentLocalId(y.AddressPersistentLocalId))
                            .ToList()))
                    .ToListAsync();
            }

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

        public async Task<bool> HasIncompleteLambert2008Geometry()
            => await ParcelsMissingLambert2008Geometry.AnyAsync();

        /// <summary>
        /// <see cref="HasIncompleteLambert2008Geometry"/> for the matching path that is synchronous end to
        /// end. Blocking on the asynchronous one from there would be sync-over-async for no gain: that path
        /// already runs its queries synchronously.
        /// </summary>
        public bool HasIncompleteLambert2008GeometrySynchronously()
            => ParcelsMissingLambert2008Geometry.Any();

        private IQueryable<ParcelConsumerItem> ParcelsMissingLambert2008Geometry
            => ParcelConsumerItemsWithCount
                .AsNoTracking()
                .Where(x => !x.IsRemoved && x.GeometryLambert2008 == null);
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
