namespace BuildingRegistry.Tests.ProjectionTests.Consumer.Address
{
    using System;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Testing;
    using BuildingRegistry.Consumer.Address;
    using Microsoft.EntityFrameworkCore;

    public abstract class ConsumerProjectionTest<TProjection>
        where TProjection : ConnectedProjection<ConsumerAddressContext>
    {
        protected ConnectedProjectionTest<ConsumerAddressContext, TProjection> Sut { get; }

        public ConsumerProjectionTest()
        {
            Sut = new ConnectedProjectionTest<ConsumerAddressContext, TProjection>(CreateContext, CreateProjection);
        }

        protected virtual ConsumerAddressContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ConsumerAddressContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ConsumerAddressContext(options);
        }

        protected abstract TProjection CreateProjection();
    }
}
