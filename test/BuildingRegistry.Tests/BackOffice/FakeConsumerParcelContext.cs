namespace BuildingRegistry.Tests.BackOffice
{
    using System;
    using System.Threading.Tasks;
    using Consumer.Read.Parcel;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.EntityFrameworkCore.Diagnostics;

    public class FakeConsumerParcelContext : ConsumerParcelContext
    {
        private bool _dontDispose = false;

        // This needs to be here to please EF
        public FakeConsumerParcelContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public FakeConsumerParcelContext(DbContextOptions<ConsumerParcelContext> options, bool dontDispose = false)
            : base(options)
        {
            _dontDispose = dontDispose;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));

            base.OnConfiguring(optionsBuilder);
        }

        public override ValueTask DisposeAsync()
        {
            if (_dontDispose)
            {
                return new ValueTask(Task.CompletedTask);
            }

            return base.DisposeAsync();
        }
    }

    public class FakeConsumerParcelContextFactory : IDesignTimeDbContextFactory<FakeConsumerParcelContext>
    {
        private readonly bool _dontDispose;

        public FakeConsumerParcelContextFactory(bool dontDispose = false)
        {
            _dontDispose = dontDispose;
        }

        public FakeConsumerParcelContext CreateDbContext(params string[] args)
        {
            var builder = new DbContextOptionsBuilder<ConsumerParcelContext>().UseInMemoryDatabase(Guid.NewGuid().ToString());

            return new FakeConsumerParcelContext(builder.Options, _dontDispose);
        }
    }
}
