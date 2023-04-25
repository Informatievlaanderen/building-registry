namespace BuildingRegistry.Tests.Grb.Handlers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using BuildingRegistry.Grb.Abstractions;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.EntityFrameworkCore.Storage;

    public class FakeBuildingGrbContext : BuildingGrbContext
    {
        public FakeDatabaseFacade FakeDatabase;

        public FakeBuildingGrbContext(
            DbContextOptions<BuildingGrbContext> options) : base(options)
        { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));

            base.OnConfiguring(optionsBuilder);
        }

        public override DatabaseFacade Database => FakeDatabase ??= new FakeDatabaseFacade(this);
    }

    public class FakeDatabaseFacade : DatabaseFacade
    {
        private IDbContextTransaction _currentTransaction;

        public FakeDatabaseFacade(DbContext context) : base(context)
        { }

        public override Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = new())
        {
            IDbContextTransaction transaction = new FakeDbContextTransaction();
            _currentTransaction = transaction;

            return Task.FromResult(transaction);
        }

        public override IDbContextTransaction? CurrentTransaction => _currentTransaction;
    }

    public class FakeDbContextTransaction : IDbContextTransaction
    {
        public TransactionStatus Status { get; private set; } = TransactionStatus.Started;

        public enum TransactionStatus
        {
            Started,
            Committed,
            Rolledback
        }

        public void Dispose() { }
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;

        public void Commit()
        {
            Status = TransactionStatus.Committed;
        }

        public Task CommitAsync(CancellationToken cancellationToken = new())
        {
            Status = TransactionStatus.Committed;
            return Task.CompletedTask;
        }

        public void Rollback()
        {
            Status = TransactionStatus.Rolledback;
        }

        public Task RollbackAsync(CancellationToken cancellationToken = new())
        {
            Status = TransactionStatus.Rolledback;
            return Task.CompletedTask;
        }

        public Guid TransactionId { get; }
    }

    public class FakeBuildingGrbContextFactory : IDesignTimeDbContextFactory<FakeBuildingGrbContext>
    {
        private readonly string _databaseName;

        public FakeBuildingGrbContextFactory(
            string? databaseName = null)
        {
            _databaseName = !string.IsNullOrWhiteSpace(databaseName)
                ? databaseName : Guid.NewGuid().ToString();
        }

        public FakeBuildingGrbContext CreateDbContext(params string[] args)
        {
            var options = new DbContextOptionsBuilder<BuildingGrbContext>()
                .UseInMemoryDatabase(_databaseName)
                .Options;

            return new FakeBuildingGrbContext(options);
        }
    }
}
