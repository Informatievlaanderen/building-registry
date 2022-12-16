namespace BuildingRegistry.Tests.ProjectionTests.Migrator
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.BackOffice.Abstractions;
    using Autofac;
    using AutoFixture;
    using BackOffice;
    using Building;
    using BuildingRegistry.Migrator.Building.Projections;
    using BuildingRegistry.Tests.Legacy.Autofixture;
    using Consumer.Address;
    using Extensions;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;
    using BuildingUnitStatus = BuildingRegistry.Legacy.BuildingUnitStatus;

    public class MigratorConsumerProjectionTests : ConsumerProjectionTest<MigratorConsumerProjection>
    {
        private readonly Fixture _fixture;
        private readonly ILogger _logger;
        private readonly FakeBackOfficeContext _fakeBackOfficeContext;

        public MigratorConsumerProjectionTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
            _logger = new LoggerFactory().CreateLogger(typeof(MigratorConsumerProjectionTests));
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fakeBackOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext();
        }

        [Fact]
        public async Task WhenBuildingWasMigratedEventWasConsumed_ThenAddBuildingUnitAddressRelation()
        {
            var buildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(111);
            var addressPersistentLocalId = new AddressPersistentLocalId(123);

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture).WithBuildingUnit(
                    BuildingUnitStatus.Planned,
                    buildingUnitPersistentLocalId,
                    attachedAddress: new List<AddressPersistentLocalId> {addressPersistentLocalId})
                .Build();

            await Sut
                .Given(buildingWasMigrated)
                .Then(async _ =>
                {
                    var result = await _fakeBackOfficeContext.BuildingUnitAddressRelation.SingleOrDefaultAsync();
                    result.Should().NotBeNull();
                    result!.BuildingPersistentLocalId.Should().Be(new BuildingPersistentLocalId(buildingWasMigrated.BuildingPersistentLocalId));
                    result.BuildingUnitPersistentLocalId.Should().Be(buildingUnitPersistentLocalId);
                    result.AddressPersistentLocalId.Should().Be(addressPersistentLocalId);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitAddressRelationAlreadyExists_ThenNothing()
        {
            var buildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(111);
            var addressPersistentLocalId = new AddressPersistentLocalId(123);

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture).WithBuildingUnit(
                    BuildingUnitStatus.Planned,
                    buildingUnitPersistentLocalId,
                    attachedAddress: new List<AddressPersistentLocalId> {addressPersistentLocalId})
                .Build();

            await _fakeBackOfficeContext.BuildingUnitAddressRelation.AddAsync(new BuildingUnitAddressRelation(
                buildingWasMigrated.BuildingPersistentLocalId,
                buildingUnitPersistentLocalId,
                addressPersistentLocalId));

            await _fakeBackOfficeContext.SaveChangesAsync();

            await Sut
                .Given(buildingWasMigrated)
                .Then(async _ =>
                {
                    var result = await _fakeBackOfficeContext.BuildingUnitAddressRelation.SingleOrDefaultAsync();
                    result.Should().NotBeNull();
                    result!.BuildingPersistentLocalId.Should().Be(new BuildingPersistentLocalId(buildingWasMigrated.BuildingPersistentLocalId));
                    result.BuildingUnitPersistentLocalId.Should().Be(buildingUnitPersistentLocalId);
                    result.AddressPersistentLocalId.Should().Be(addressPersistentLocalId);
                });
        }

        protected override MigratorConsumerProjection CreateProjection()
        {
            var mockDbFactory = new Mock<IDbContextFactory<BackOfficeContext>>();
                mockDbFactory
                    .Setup(x => x.CreateDbContextAsync(CancellationToken.None))
                    .Returns(Task.FromResult<BackOfficeContext>(_fakeBackOfficeContext));

            return new MigratorConsumerProjection(_logger, mockDbFactory.Object);
        }

        protected override void ConfigureCommandHandling(ContainerBuilder builder)
        { }

        protected override void ConfigureEventHandling(ContainerBuilder builder)
        { }
    }
}
