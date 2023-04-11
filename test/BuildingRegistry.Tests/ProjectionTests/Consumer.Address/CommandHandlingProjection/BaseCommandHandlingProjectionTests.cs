namespace BuildingRegistry.Tests.ProjectionTests.Consumer.Address.CommandHandlingProjection
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.BackOffice.Abstractions;
    using BuildingRegistry.Consumer.Address.Projections;
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using Tests.BackOffice;
    using Tests.Legacy.Autofixture;
    using Xunit.Abstractions;

    public class BaseCommandHandlingProjectionTests : KafkaProjectionTest<CommandHandler, CommandHandlingKafkaProjection>
    {
        protected readonly FakeBackOfficeContext FakeBackOfficeContext;
        protected readonly Mock<FakeCommandHandler> MockCommandHandler;

        public BaseCommandHandlingProjectionTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());

            MockCommandHandler = new Mock<FakeCommandHandler>();
            FakeBackOfficeContext = new FakeBackOfficeContextFactory(true).CreateDbContext(Array.Empty<string>());
        }

        protected override CommandHandler CreateContext()
        {
            return MockCommandHandler.Object;
        }

        protected override CommandHandlingKafkaProjection CreateProjection()
        {
            var factoryMock = new Mock<IDbContextFactory<BackOfficeContext>>();
            factoryMock
                .Setup(x => x.CreateDbContextAsync(CancellationToken.None))
                .Returns(Task.FromResult<BackOfficeContext>(FakeBackOfficeContext));
            return new CommandHandlingKafkaProjection(factoryMock.Object);
        }
    }
}
