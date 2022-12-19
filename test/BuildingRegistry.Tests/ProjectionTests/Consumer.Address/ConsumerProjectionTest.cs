namespace BuildingRegistry.Tests.ProjectionTests.Consumer.Address
{
    using System;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Testing;
    using BuildingRegistry.Migrator.Building.Projections;
    using Microsoft.EntityFrameworkCore;
    using Xunit.Abstractions;

    public abstract class ConsumerProjectionTest<TProjection>  : BuildingRegistryTest
        where TProjection : ConnectedProjection<MigratorProjectionContext>
    {
        protected ConnectedProjectionTest<MigratorProjectionContext, TProjection> Sut { get; }

        public ConsumerProjectionTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Sut = new ConnectedProjectionTest<MigratorProjectionContext, TProjection>(CreateContext, CreateProjection);
        }

        protected virtual MigratorProjectionContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<MigratorProjectionContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new MigratorProjectionContext(options);
        }

        protected abstract TProjection CreateProjection();
    }
}
