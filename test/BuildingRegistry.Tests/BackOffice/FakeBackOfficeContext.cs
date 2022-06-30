namespace BuildingRegistry.Tests.BackOffice
{
    using System;
    using BuildingRegistry.Api.BackOffice.Abstractions;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;

    public class FakeBackOfficeContext : BackOfficeContext
    {
        // This needs to be here to please EF
        public FakeBackOfficeContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public FakeBackOfficeContext(DbContextOptions<BackOfficeContext> options)
            : base(options) { }
    }

    public class FakeBackOfficeContextFactory : IDesignTimeDbContextFactory<FakeBackOfficeContext>
    {
        public FakeBackOfficeContext CreateDbContext(params string[] args)
        {
            var builder = new DbContextOptionsBuilder<BackOfficeContext>().UseInMemoryDatabase(Guid.NewGuid().ToString());
            return new FakeBackOfficeContext(builder.Options);
        }
    }
}
