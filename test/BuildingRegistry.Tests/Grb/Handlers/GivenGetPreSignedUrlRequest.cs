namespace BuildingRegistry.Tests.Grb.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using BuildingRegistry.Api.Grb.Infrastructure;
    using BuildingRegistry.Api.Grb.Infrastructure.Options;
    using BuildingRegistry.Api.Grb.Uploads;
    using BuildingRegistry.Grb.Abstractions;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Moq;
    using TicketingService.Abstractions;
    using Xunit;

    public class GivenGetPreSignedUrlRequest
    {
        private readonly Fixture _fixture;

        private readonly Mock<ITicketingUrl> _ticketingUrl;
        private readonly Mock<ITicketing> _ticketing;
        private readonly Mock<IAmazonS3Extended> _s3Extended;

        public GivenGetPreSignedUrlRequest()
        {
            _fixture = new Fixture();

            _ticketingUrl = new Mock<ITicketingUrl>();
            _ticketing = new Mock<ITicketing>();
            _s3Extended = new Mock<IAmazonS3Extended>();
        }

        private UploadPreSignedUrlHandler CreatePreSignedUrlHandler(BuildingGrbContext buildingGrbContext)
        {
            return new UploadPreSignedUrlHandler(
                buildingGrbContext,
                _ticketing.Object,
                _ticketingUrl.Object,
                _s3Extended.Object,
                Options.Create(new BucketOptions
                {
                    BucketName = "Test",
                    UrlExpirationInMinutes = 60
                }));
        }

        [Fact]
        public async Task ReturnsGetPreSignedUrlResponse()
        {
            var ticketId = Guid.NewGuid();
            var ticketUrl = $"https://api.ticketing.vlaanderen.be/{ticketId}";
            var preSignedUrl = new Uri("https://signedUrl");

            _ticketing
                .Setup(x => x.CreateTicket(It.IsAny<IDictionary<string, string>>(), CancellationToken.None))
                .ReturnsAsync(ticketId);
            _ticketingUrl
                .Setup(x => x.For(ticketId))
                .Returns(new Uri(ticketUrl));
            _s3Extended
                .Setup(x => x.CreatePresignedPost(It.IsAny<CreatePresignedPostRequest>()))
                .Returns(new CreatePresignedPostResponse(preSignedUrl, _fixture.Create<Dictionary<string, string>>()));

            var databaseName = nameof(ReturnsGetPreSignedUrlResponse);
            var buildingGrbContext = new FakeBuildingGrbContextFactory(databaseName).CreateDbContext(Array.Empty<string>());
            var handler = CreatePreSignedUrlHandler(buildingGrbContext);
            var response = await handler.Handle(new UploadPreSignedUrlRequest(), CancellationToken.None);

            var job = await buildingGrbContext.Jobs.SingleOrDefaultAsync();

            job.Should().NotBeNull();
            job!.TicketId.Should().Be(ticketId);
            job.Status.Should().Be(JobStatus.Created);
            response.JobId.Should().Be(job.Id);
            response.TicketUrl.Should().Be(ticketUrl);
            response.UploadUrl.Should().Be(preSignedUrl.ToString());

            var transaction = (FakeDbContextTransaction)buildingGrbContext.Database.CurrentTransaction;
            transaction.Status.Should().Be(FakeDbContextTransaction.TransactionStatus.Committed);
        }

        [Fact]
        public async Task WhenError_ThenNoJobIsCreated()
        {
            _ticketing
                .Setup(x => x.CreateTicket(It.IsAny<IDictionary<string, string>>(), CancellationToken.None))
                .ThrowsAsync(new Exception());

            var databaseName = nameof(WhenError_ThenNoJobIsCreated);
            await using var buildingGrbContext = new FakeBuildingGrbContextFactory(databaseName).CreateDbContext(Array.Empty<string>());
            var handler = CreatePreSignedUrlHandler(buildingGrbContext);

            try
            {
                await handler.Handle(new UploadPreSignedUrlRequest(), CancellationToken.None);
            }
            catch (Exception)
            {
            }

            var transaction = (FakeDbContextTransaction)buildingGrbContext.Database.CurrentTransaction;
            transaction.Status.Should().Be(FakeDbContextTransaction.TransactionStatus.Rolledback);
        }
    }
}
