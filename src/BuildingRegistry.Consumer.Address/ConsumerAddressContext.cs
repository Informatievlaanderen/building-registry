namespace BuildingRegistry.Consumer.Address
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer.SqlServer;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.SqlServer.MigrationExtensions;
    using Building;
    using Building.Datastructures;
    using BuildingRegistry.Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.Extensions.Configuration;

    public class ConsumerAddressContext : SqlServerConsumerDbContext<ConsumerAddressContext>, IAddresses
    {
        public DbSet<AddressConsumerItem> AddressConsumerItems { get; set; }

        // This needs to be here to please EF
        public ConsumerAddressContext()
        { }

        // This needs to be DbContextOptions<T> for Autofac!
        public ConsumerAddressContext(DbContextOptions<ConsumerAddressContext> options)
            : base(options)
        { }

        public override string ProcessedMessagesSchema => Schema.ConsumerAddress;

        public AddressData? GetOptional(AddressPersistentLocalId addressPersistentLocalId)
        {
            var item = AddressConsumerItems
                .AsNoTracking()
                .SingleOrDefault(x => x.AddressPersistentLocalId == addressPersistentLocalId);

            if (item is null)
            {
                return null;
            }

            return new AddressData(new AddressPersistentLocalId(item.AddressPersistentLocalId), Map(item.Status), item.IsRemoved);
        }

        public async Task<List<AddressData>> GetAddresses(List<AddressPersistentLocalId> addressPersistentLocalIds)
        {
            var items = AddressConsumerItems
                .AsNoTracking()
                .Where(x => addressPersistentLocalIds.Contains(new AddressPersistentLocalId(x.AddressPersistentLocalId)));

            return await items
                .Select(x => new AddressData(new AddressPersistentLocalId(x.AddressPersistentLocalId), Map(x.Status), x.IsRemoved))
                .ToListAsync();
        }

        public BuildingRegistry.Building.Datastructures.AddressStatus Map(AddressStatus status)
        {
            if (status == AddressStatus.Proposed)
            {
                return BuildingRegistry.Building.Datastructures.AddressStatus.Proposed;
            }
            if (status == AddressStatus.Current)
            {
                return BuildingRegistry.Building.Datastructures.AddressStatus.Current;
            }
            if (status == AddressStatus.Rejected)
            {
                return BuildingRegistry.Building.Datastructures.AddressStatus.Rejected;
            }
            if (status == AddressStatus.Retired)
            {
                return BuildingRegistry.Building.Datastructures.AddressStatus.Retired;
            }

            throw new NotImplementedException($"Cannot parse {status} to AddressStatus");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ConsumerAddressContext).GetTypeInfo().Assembly);
        }
    }

    public class ConsumerContextFactory : IDesignTimeDbContextFactory<ConsumerAddressContext>
    {
        public ConsumerAddressContext CreateDbContext(string[] args)
        {
            const string migrationConnectionStringName = "ConsumerAddressAdmin";

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            var builder = new DbContextOptionsBuilder<ConsumerAddressContext>();

            var connectionString = configuration.GetConnectionString(migrationConnectionStringName);
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException($"Could not find a connection string with name '{migrationConnectionStringName}'");

            builder
                .UseSqlServer(connectionString, sqlServerOptions =>
                {
                    sqlServerOptions.EnableRetryOnFailure();
                    sqlServerOptions.MigrationsHistoryTable(MigrationTables.ConsumerAddress, Schema.ConsumerAddress);
                })
                .UseExtendedSqlServerMigrations();

            return new ConsumerAddressContext(builder.Options);
        }
    }
}
