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
    using Parlot.Fluent;
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
                    Fixture.Create<string>(),
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

        [Fact]
        public async Task ParcelWasRetiredV2_AddsParcelIfNotExists()
        {
            var parcelWasRetiredV2 = Fixture
                .Build<ParcelWasRetiredV2>()
                .FromFactory(() => new ParcelWasRetiredV2(
                    Fixture.Create<Guid>().ToString("D"),
                    Fixture.Create<Guid>().ToString("D"),
                    Fixture.Create<Provenance>()
                ))
                .Create();

            Given(parcelWasRetiredV2);

            await Then(async context =>
            {
                var parcel =
                    await context.ParcelConsumerItems.FindAsync(Guid.Parse(parcelWasRetiredV2.ParcelId));

                parcel.Should().NotBeNull();
                parcel!.CaPaKey.Should().Be(parcelWasRetiredV2.CaPaKey);
                parcel.Status.Should().Be(ParcelStatus.Retired);
                parcel.IsRemoved.Should().Be(parcel.IsRemoved);
            });
        }

        [Fact]
        public async Task ParcelWasRetiredV2_SetsParcelStatusToRetired()
        {
            var parcelId = Fixture.Create<Guid>().ToString("D");
            var capakey = Fixture.Create<Guid>().ToString("D");

            var parcelWasImported = Fixture
                .Build<ParcelWasImported>()
                .FromFactory(() => new ParcelWasImported(
                    parcelId,
                    capakey,
                    Fixture.Create<string>(),
                    Fixture.Create<Provenance>()
                ))
                .Create();

            var parcelWasRetiredV2 = Fixture
                .Build<ParcelWasRetiredV2>()
                .FromFactory(() => new ParcelWasRetiredV2(
                    parcelId,
                    capakey,
                    Fixture.Create<Provenance>()
                ))
                .Create();

            Given(parcelWasImported, parcelWasRetiredV2);

            await Then(async context =>
            {
                var parcel =
                    await context.ParcelConsumerItems.FindAsync(Guid.Parse(parcelWasRetiredV2.ParcelId));

                parcel.Should().NotBeNull();
                parcel!.CaPaKey.Should().Be(parcelWasRetiredV2.CaPaKey);
                parcel.Status.Should().Be(ParcelStatus.Retired);
                parcel.IsRemoved.Should().Be(parcel.IsRemoved);
            });
        }

        [Fact]
        public async Task ParcelWasCorrectedFromRetiredToRealized_AddsParcelIfNotExists()
        {
            var parcelWasCorrectedFromRetiredToRealized = Fixture
                .Build<ParcelWasCorrectedFromRetiredToRealized>()
                .FromFactory(() => new ParcelWasCorrectedFromRetiredToRealized(
                    Fixture.Create<Guid>().ToString("D"),
                    Fixture.Create<Guid>().ToString("D"),
                    Fixture.Create<string>(),
                    Fixture.Create<Provenance>()
                ))
                .Create();

            Given(parcelWasCorrectedFromRetiredToRealized);

            await Then(async context =>
            {
                var parcel =
                    await context.ParcelConsumerItems.FindAsync(Guid.Parse(parcelWasCorrectedFromRetiredToRealized.ParcelId));

                parcel.Should().NotBeNull();
                parcel!.CaPaKey.Should().Be(parcelWasCorrectedFromRetiredToRealized.CaPaKey);
                parcel.Status.Should().Be(ParcelStatus.Realized);
                parcel.IsRemoved.Should().Be(parcel.IsRemoved);
            });
        }

        [Fact]
        public async Task ParcelWasCorrectedFromRetiredToRealized_SetsParcelStatusToRealized()
        {
            var parcelId = Fixture.Create<Guid>().ToString("D");
            var capakey = Fixture.Create<Guid>().ToString("D");
            var parcelWasImported = Fixture
                .Build<ParcelWasImported>()
                .FromFactory(() => new ParcelWasImported(
                    parcelId,
                    capakey,
                    Fixture.Create<string>(),
                    Fixture.Create<Provenance>()
                ))
                .Create();

            var parcelWasRetiredV2 = Fixture
                .Build<ParcelWasRetiredV2>()
                .FromFactory(() => new ParcelWasRetiredV2(
                    parcelId,
                    capakey,
                    Fixture.Create<Provenance>()
                ))
                .Create();

            var parcelWasCorrectedFromRetiredToRealized = Fixture
                .Build<ParcelWasCorrectedFromRetiredToRealized>()
                .FromFactory(() => new ParcelWasCorrectedFromRetiredToRealized(
                    parcelId,
                    capakey,
                    Fixture.Create<string>(),
                    Fixture.Create<Provenance>()
                ))
                .Create();



            Given(parcelWasImported, parcelWasRetiredV2, parcelWasCorrectedFromRetiredToRealized);

            await Then(async context =>
            {
                var parcel =
                    await context.ParcelConsumerItems.FindAsync(Guid.Parse(parcelWasCorrectedFromRetiredToRealized.ParcelId));

                parcel.Should().NotBeNull();
                parcel!.CaPaKey.Should().Be(parcelWasCorrectedFromRetiredToRealized.CaPaKey);
                parcel.Status.Should().Be(ParcelStatus.Realized);
                parcel.IsRemoved.Should().Be(parcel.IsRemoved);
            });
        }

        [Fact]
        public async Task ParcelWasImported_AddsParcel()
        {
            var parcelWasImported = Fixture
                .Build<ParcelWasImported>()
                .FromFactory(() => new ParcelWasImported(
                    Fixture.Create<Guid>().ToString("D"),
                    Fixture.Create<Guid>().ToString("D"),
                    Fixture.Create<string>(),
                    Fixture.Create<Provenance>()
                ))
                .Create();

            Given(parcelWasImported);

            await Then(async context =>
            {
                var parcel =
                    await context.ParcelConsumerItems.FindAsync(Guid.Parse(parcelWasImported.ParcelId));

                parcel.Should().NotBeNull();
                parcel!.CaPaKey.Should().Be(parcelWasImported.CaPaKey);
                parcel.Status.Should().Be(ParcelStatus.Realized);
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
