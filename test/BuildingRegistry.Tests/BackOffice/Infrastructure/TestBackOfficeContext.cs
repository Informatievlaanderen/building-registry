namespace BuildingRegistry.Tests.BackOffice.Infrastructure
{
    using System;
    using BuildingRegistry.Api.BackOffice;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;

    public class TestBackOfficeContext : BackOfficeContext
    {
        // This needs to be here to please EF
        public TestBackOfficeContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public TestBackOfficeContext(DbContextOptions<BackOfficeContext> options)
            : base(options) { }

        //TODO: when buildingunits come into play
        //public AddressPersistentIdStreetNamePersistentId AddStreetNamePersistentIdByAddressPersistentLocalIdToFixture(
        //    int persistentLocalId, int streetNamePersistentLocalId)
        //{
        //    var item = new Fixture().Create<AddressPersistentIdStreetNamePersistentId>();

        //    item.AddressPersistentLocalId = persistentLocalId;
        //    item.StreetNamePersistentLocalId = streetNamePersistentLocalId;

        //    AddressPersistentIdStreetNamePersistentIds.Add(item);
        //    SaveChanges();
        //    return item;
        //}
    }

    public class FakeBackOfficeContextFactory : IDesignTimeDbContextFactory<TestBackOfficeContext>
    {
        public TestBackOfficeContext CreateDbContext(params string[] args)
        {
            var builder = new DbContextOptionsBuilder<BackOfficeContext>().UseInMemoryDatabase(Guid.NewGuid().ToString());
            return new TestBackOfficeContext(builder.Options);
        }
    }
}
