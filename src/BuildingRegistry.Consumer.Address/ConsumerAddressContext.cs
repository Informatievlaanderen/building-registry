namespace BuildingRegistry.Consumer.Address
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using BuildingRegistry.Infrastructure;
    using Microsoft.EntityFrameworkCore;

    public class ConsumerAddressContext : RunnerDbContext<ConsumerAddressContext>
    {
        public DbSet<AddressConsumerItem> AddressConsumerItems { get; set; }

        // This needs to be here to please EF
        public ConsumerAddressContext()
        { }

        // This needs to be DbContextOptions<T> for Autofac!
        public ConsumerAddressContext(DbContextOptions<ConsumerAddressContext> options)
            : base(options)
        { }

        public override string ProjectionStateSchema => Schema.ConsumerAddress;
    }

    public class ConsumerContextFactory : RunnerDbContextMigrationFactory<ConsumerAddressContext>
    {
        public ConsumerContextFactory()
            : this("ConsumerAddressAdmin")
        { }

        public ConsumerContextFactory(string connectionStringName)
            : base(connectionStringName, new MigrationHistoryConfiguration
            {
                Schema = Schema.ConsumerAddress,
                Table = MigrationTables.ConsumerAddress
            })
        { }

        protected override ConsumerAddressContext CreateContext(DbContextOptions<ConsumerAddressContext> migrationContextOptions) => new ConsumerAddressContext(migrationContextOptions);

        public ConsumerAddressContext Create(DbContextOptions<ConsumerAddressContext> options) => CreateContext(options);
    }
}
