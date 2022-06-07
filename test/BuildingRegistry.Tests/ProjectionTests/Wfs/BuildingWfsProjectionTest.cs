namespace BuildingRegistry.Tests.ProjectionTests.Wfs
{
    using System;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Testing;
    using Microsoft.EntityFrameworkCore;
    using Projections.Wfs;

    public abstract class BuildingWfsProjectionTest<TProjection>
        where TProjection : ConnectedProjection<WfsContext>
    {
        protected ConnectedProjectionTest<WfsContext, TProjection> Sut { get; }

        protected BuildingWfsProjectionTest()
        {
            Sut = new ConnectedProjectionTest<WfsContext, TProjection>(CreateContext, CreateProjection);
        }

        protected virtual WfsContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<WfsContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new WfsContext(options);
        }

        protected abstract TProjection CreateProjection();
    }
}
