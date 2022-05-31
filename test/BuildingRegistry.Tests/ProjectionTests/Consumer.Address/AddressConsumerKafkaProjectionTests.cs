namespace BuildingRegistry.Tests.ProjectionTests.Consumer.Address
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.AddressRegistry;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.Common;
    using BuildingRegistry.Consumer.Address;
    using BuildingRegistry.Consumer.Address.Projections;
    using FluentAssertions;
    using Legacy.Autofixture;
    using Microsoft.EntityFrameworkCore;
    using Xunit;
    using Xunit.Abstractions;

    public class AddressConsumerKafkaProjectionTests : KafkaProjectionTest<ConsumerAddressContext, AddressKafkaProjection>
    {
        private readonly Fixture _fixture;

        public AddressConsumerKafkaProjectionTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
        }

        [Fact]
        public async Task AddressMigratedToStreetName_AddsAddress()
        {
            var addressStatus = _fixture
                .Build<AddressStatus>()
                .FromFactory(() =>
                {
                    var statusses = new List<AddressStatus>
                    {
                        AddressStatus.Current, AddressStatus.Proposed, AddressStatus.Rejected, AddressStatus.Retired
                    };

                    return statusses[new Random(_fixture.Create<int>()).Next(0, statusses.Count - 1)];
                })
                .Create();

            var addressWasMigratedToStreetName = _fixture
                .Build<AddressWasMigratedToStreetName>()
                .FromFactory(() => new AddressWasMigratedToStreetName(
                    _fixture.Create<int>(),
                    _fixture.Create<Guid>().ToString("D"),
                    _fixture.Create<Guid>().ToString("D"),
                    _fixture.Create<int>(),
                    addressStatus.Status,
                    _fixture.Create<string>(),
                    _fixture.Create<string>(),
                    _fixture.Create<string>(),
                    _fixture.Create<string>(),
                    _fixture.Create<string>(),
                    _fixture.Create<bool>(),
                    _fixture.Create<string>(),
                    _fixture.Create<bool>(),
                    _fixture.Create<bool>(),
                    _fixture.Create<int?>(),
                    _fixture.Create<Provenance>()
                    ))
                .Create();

            Given(addressWasMigratedToStreetName);

            await Then(async context =>
            {
                var address =
                    await context.AddressConsumerItems.FindAsync(
                        addressWasMigratedToStreetName.AddressPersistentLocalId);

                address.Should().NotBeNull();
                address.AddressId.Should().Be(Guid.Parse(addressWasMigratedToStreetName.AddressId));
                address.IsRemoved.Should().Be(address.IsRemoved);
                address.Status.Should().Be(addressStatus);
            });
        }

        [Fact]
        public async Task AddressWasProposedV2_AddsAddress()
        {
            var addressWasProposed = _fixture.Create<AddressWasProposedV2>();
            Given(addressWasProposed);

            await Then(async context =>
            {
                var address =
                    await context.AddressConsumerItems.FindAsync(
                        addressWasProposed.AddressPersistentLocalId);

                address.Should().NotBeNull();
                address.AddressId.Should().BeNull();
                address.IsRemoved.Should().Be(false);
                address.Status.Should().Be(AddressStatus.Proposed);
            });
        }

        [Fact]
        public async Task AddressWasApproved_UpdatesStatusAddress()
        {
            var addressWasProposed = _fixture.Create<AddressWasProposedV2>();
            var addressWasApproved = _fixture.Build<AddressWasApproved>()
                .FromFactory(() => new AddressWasApproved(addressWasProposed.StreetNamePersistentLocalId, addressWasProposed.AddressPersistentLocalId, _fixture.Create<Provenance>()))
                .Create();

            Given(addressWasProposed, addressWasApproved);

            await Then(async context =>
            {
                var address =
                    await context.AddressConsumerItems.FindAsync(
                        addressWasApproved.AddressPersistentLocalId);

                address.Should().NotBeNull();
                address.Status.Should().Be(AddressStatus.Current);
            });
        }

        protected override ConsumerAddressContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ConsumerAddressContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ConsumerAddressContext(options);
        }

        protected override AddressKafkaProjection CreateProjection() => new AddressKafkaProjection();
    }
}
