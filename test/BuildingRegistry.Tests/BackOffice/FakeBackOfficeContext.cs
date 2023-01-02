namespace BuildingRegistry.Tests.BackOffice
{
    using System;
    using System.Threading.Tasks;
    using Building;
    using BuildingRegistry.Api.BackOffice.Abstractions;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.EntityFrameworkCore.Diagnostics;

    public class FakeBackOfficeContext : BackOfficeContext
    {
        private bool _dontDispose = false;

        // This needs to be here to please EF
        public FakeBackOfficeContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public FakeBackOfficeContext(DbContextOptions<BackOfficeContext> options, bool dontDispose = false)
            : base(options)
        {
            _dontDispose = dontDispose;
        }

        public async Task AddBuildingUnitBuilding(
            BuildingPersistentLocalId buildingPersistentLocalId,
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId)
        {
            BuildingUnitBuildings.Add(new BuildingUnitBuilding(buildingUnitPersistentLocalId, buildingPersistentLocalId));
            await SaveChangesAsync();
        }

        public async Task AddBuildingUnitAddressRelation(
            BuildingPersistentLocalId buildingPersistentLocalId,
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            AddressPersistentLocalId addressPersistentLocalId)
        {
            BuildingUnitAddressRelation.Add(new BuildingUnitAddressRelation(buildingPersistentLocalId, buildingUnitPersistentLocalId, addressPersistentLocalId));
            await SaveChangesAsync();
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

    public class FakeBackOfficeContextFactory : IDesignTimeDbContextFactory<FakeBackOfficeContext>
    {
        private readonly bool _dontDispose;

        public FakeBackOfficeContextFactory(bool dontDispose = false)
        {
            _dontDispose = dontDispose;
        }

        public FakeBackOfficeContext CreateDbContext(params string[] args)
        {
            var builder = new DbContextOptionsBuilder<BackOfficeContext>().UseInMemoryDatabase(Guid.NewGuid().ToString());

            return new FakeBackOfficeContext(builder.Options, _dontDispose);
        }
    }
}
