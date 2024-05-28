namespace BuildingRegistry.Tests.ProjectionTests.BackOffice
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Testing;
    using BuildingRegistry.Api.BackOffice.Abstractions;
    using BuildingRegistry.Projections.BackOffice;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Moq;

    public abstract class BuildingBackOfficeProjectionsTest
    {
        protected ConnectedProjectionTest<BackOfficeProjectionsContext, BackOfficeProjections> Sut { get; }
        protected Mock<IDbContextFactory<BackOfficeContext>> BackOfficeContextMock { get; }

        protected BuildingBackOfficeProjectionsTest()
        {
            BackOfficeContextMock = new Mock<IDbContextFactory<BackOfficeContext>>();
            Sut = new ConnectedProjectionTest<BackOfficeProjectionsContext, BackOfficeProjections>(
                CreateContext,
                () => new BackOfficeProjections(BackOfficeContextMock.Object, new ConfigurationBuilder().Build()));
        }

        protected virtual BackOfficeProjectionsContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<BackOfficeProjectionsContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new BackOfficeProjectionsContext(options);
        }

        protected Envelope<TMessage> BuildEnvelope<TMessage>(TMessage message)
            where TMessage : IMessage
        {
            return new Envelope<TMessage>(new Envelope(message, new Dictionary<string, object>
            {
                { Envelope.CreatedUtcMetadataKey, DateTime.UtcNow }
            }));
        }
    }
}
