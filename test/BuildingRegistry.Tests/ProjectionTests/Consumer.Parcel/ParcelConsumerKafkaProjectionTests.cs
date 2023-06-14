namespace BuildingRegistry.Tests.ProjectionTests.Consumer.Parcel
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.ParcelRegistry;
    using BuildingRegistry.Consumer.Read.Parcel;
    using BuildingRegistry.Consumer.Read.Parcel.Projections;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore;
    using Tests.Legacy.Autofixture;
    using Xunit;
    using Xunit.Abstractions;

    public class ParcelConsumerKafkaProjectionTests : KafkaProjectionTest<ConsumerParcelContext, ParcelKafkaProjection>
    {
        public ParcelConsumerKafkaProjectionTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
        }

        [Fact]
        public async Task ParcelWasMigrated_AddsParcel()
        {
            var parcelStatus = Fixture
                .Build<ParcelStatus>()
                .FromFactory(() =>
                {
                    var statuses = new List<ParcelStatus>
                    {
                        ParcelStatus.Realized, ParcelStatus.Retired
                    };

                    return statuses[new Random(Fixture.Create<int>()).Next(0, statuses.Count - 1)];
                })
                .Create();

            var parcelWasMigrated = Fixture
                .Build<ParcelWasMigrated>()
                .FromFactory(() => new ParcelWasMigrated(
                    Fixture.Create<Guid>().ToString("D"),
                    Fixture.Create<Guid>().ToString("D"),
                    Fixture.Create<string>(),
                    parcelStatus.Status,
                    Fixture.Create<bool>(),
                    Fixture.Create<IEnumerable<int>>(),
                    Fixture.Create<decimal?>(),
                    Fixture.Create<decimal?>(),
                    Fixture.Create<Provenance>()
                ))
                .Create();

            Given(parcelWasMigrated);

            await Then(async context =>
            {
                var parcel =
                    await context.ParcelConsumerItems.FindAsync(Guid.Parse(parcelWasMigrated.ParcelId));

                parcel.Should().NotBeNull();
                parcel!.CaPaKey.Should().Be(parcelWasMigrated.CaPaKey);
                parcel.Status.Should().Be(parcelStatus);
                parcel.IsRemoved.Should().Be(parcel.IsRemoved);
            });
        }

        protected override ConsumerParcelContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ConsumerParcelContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ConsumerParcelContext(options);
        }

        protected override ParcelKafkaProjection CreateProjection() => new ParcelKafkaProjection();
    }
}
