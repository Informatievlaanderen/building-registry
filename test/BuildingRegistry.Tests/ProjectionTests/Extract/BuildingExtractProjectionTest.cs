namespace BuildingRegistry.Tests.ProjectionTests.Extract
{
    using System;
    using System.Text;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Testing;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Projections.Extract;

    public abstract class BuildingExtractProjectionTest<TProjection>
        where TProjection : ConnectedProjection<ExtractContext>
    {
        protected ConnectedProjectionTest<ExtractContext, TProjection> Sut { get; }

        protected BuildingExtractProjectionTest()
        {
            Sut = new ConnectedProjectionTest<ExtractContext, TProjection>(CreateContext, CreateProjection);
        }

        protected static IOptions<ExtractConfig> ExtractConfig { get; } =
            new OptionsWrapper<ExtractConfig>(new ExtractConfig());

        protected static Encoding Encoding { get; } = Encoding.UTF8;

        protected virtual ExtractContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ExtractContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ExtractContext(options);
        }

        protected abstract TProjection CreateProjection();
    }
}
