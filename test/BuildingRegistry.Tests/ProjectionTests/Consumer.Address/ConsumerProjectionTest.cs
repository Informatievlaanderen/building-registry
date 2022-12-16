namespace BuildingRegistry.Tests.ProjectionTests.Consumer.Address
{
    using System;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Testing;
    using BuildingRegistry.Migrator.Building.Projections;
    using Microsoft.EntityFrameworkCore;
    using Xunit.Abstractions;

    public abstract class ConsumerProjectionTest<TProjection>  : BuildingRegistryTest
        where TProjection : ConnectedProjection<MigratorConsumerContext>
    {
        protected ConnectedProjectionTest<MigratorConsumerContext, TProjection> Sut { get; }

        public ConsumerProjectionTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Sut = new ConnectedProjectionTest<MigratorConsumerContext, TProjection>(CreateContext, CreateProjection);
        }

        protected virtual MigratorConsumerContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<MigratorConsumerContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new MigratorConsumerContext(options);
        }

        protected abstract TProjection CreateProjection();
    }
}
