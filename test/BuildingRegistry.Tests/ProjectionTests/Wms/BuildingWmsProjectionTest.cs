namespace BuildingRegistry.Tests.ProjectionTests.Wms
{
    using System;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Testing;
    using Microsoft.EntityFrameworkCore;
    using Projections.Wms;

    public abstract class BuildingWmsProjectionTest<TProjection>
        where TProjection : ConnectedProjection<WmsContext>
    {
        protected ConnectedProjectionTest<WmsContext, TProjection> Sut { get; }

        protected BuildingWmsProjectionTest()
        {
            Sut = new ConnectedProjectionTest<WmsContext, TProjection>(CreateContext, CreateProjection);
        }

        protected virtual WmsContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<WmsContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new WmsContext(options);
        }

        protected abstract TProjection CreateProjection();
    }
}
