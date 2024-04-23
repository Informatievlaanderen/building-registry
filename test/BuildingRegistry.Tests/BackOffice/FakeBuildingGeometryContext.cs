namespace BuildingRegistry.Tests.BackOffice
{
    using System;
    using System.Threading.Tasks;
    using BuildingRegistry.Api.BackOffice.Handlers.Lambda;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.EntityFrameworkCore.Diagnostics;

    public class FakeBuildingGeometryContext : BuildingGeometryContext
    {
        private bool _dontDispose = false;

        // This needs to be here to please EF
        public FakeBuildingGeometryContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public FakeBuildingGeometryContext(DbContextOptions<BuildingGeometryContext> options, bool dontDispose = false)
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

    public class FakeBuildingGeometryContextFactory : IDesignTimeDbContextFactory<FakeBuildingGeometryContext>
    {
        private readonly bool _dontDispose;

        public FakeBuildingGeometryContextFactory(bool dontDispose = false)
        {
            _dontDispose = dontDispose;
        }

        public FakeBuildingGeometryContext CreateDbContext(params string[] args)
        {
            var builder = new DbContextOptionsBuilder<BuildingGeometryContext>().UseInMemoryDatabase(Guid.NewGuid().ToString());

            return new FakeBuildingGeometryContext(builder.Options, _dontDispose);
        }
    }
}
