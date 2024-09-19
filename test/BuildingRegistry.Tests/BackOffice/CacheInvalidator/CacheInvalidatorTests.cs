namespace BuildingRegistry.Tests.BackOffice.CacheInvalidator
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Building;
    using Cache.Invalidator;
    using Consumer.Read.Parcel;
    using FluentAssertions;
    using Microsoft.Extensions.Hosting;
    using Moq;
    using Xunit;

    public class CacheInvalidatorTests
    {
        [Fact]
        public async Task GivenBuildingsToInvalidate_ThenShouldInvalidateCache()
        {
            // Arrange
            var hostApplicationLifeTimeMock = new Mock<IHostApplicationLifetime>();
            var redisCacheInvalidateService = new Mock<IRedisCacheInvalidateService>();
            var consumerParcelContext = new FakeConsumerParcelContextFactory()
                .CreateDbContext([]);
            consumerParcelContext.BuildingsToInvalidate.Add(new BuildingToInvalidate
            {
                BuildingPersistentLocalId = 1
            });
            consumerParcelContext.BuildingsToInvalidate.Add(new BuildingToInvalidate
            {
                BuildingPersistentLocalId = 2
            });
            await consumerParcelContext.SaveChangesAsync();

            // Act
            var service = new CacheInvalidator(hostApplicationLifeTimeMock.Object, consumerParcelContext, redisCacheInvalidateService.Object);
            await service.StartAsync(CancellationToken.None);

            // Assert
            consumerParcelContext.BuildingsToInvalidate.Should().BeEmpty();
            redisCacheInvalidateService.Invocations.Should().HaveCount(1);
            redisCacheInvalidateService.Invocations.Single().Arguments.Single().Should().BeEquivalentTo(new [] {
                new BuildingPersistentLocalId(1),
                new BuildingPersistentLocalId(2)
            });
        }
    }
}
