namespace BuildingRegistry.Tests.Grb.Handlers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon.S3.Model;
    using Api.Grb.Infrastructure;
    using Api.Grb.Infrastructure.Options;
    using Api.Grb.Uploads;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using BuildingRegistry.Grb.Abstractions;
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;
    using Moq;
    using Xunit;

    public class GivenJobResultPreSignedUrlRequest
    {
        private readonly FakeBuildingGrbContext _fakeBuildingGrbContext;
        private readonly BucketOptions _bucketOptions;
        private readonly Mock<IAmazonS3Extended> _s3Extended;
        private readonly JobResultsPreSignedUrlHandler _handler;

        public GivenJobResultPreSignedUrlRequest()
        {
            _fakeBuildingGrbContext = new FakeBuildingGrbContextFactory().CreateDbContext();
            _s3Extended = new Mock<IAmazonS3Extended>();
            _bucketOptions = new BucketOptions { BucketName = "Test", UrlExpirationInMinutes = 60 };
            _handler = new JobResultsPreSignedUrlHandler(
                _fakeBuildingGrbContext,
                Options.Create(_bucketOptions),
                _s3Extended.Object);
        }

        [Fact]
        public async Task ThenReturnsPresignedUrlResponse()
        {
            var job = new Job(DateTimeOffset.Now, JobStatus.Completed) { Id = Guid.NewGuid() };
            _fakeBuildingGrbContext.Jobs.Add(job);
            await _fakeBuildingGrbContext.SaveChangesAsync();

            const string expectedPresignedUrl = "https://presignedurl.com";

            _s3Extended
                .Setup(x => x.GetPreSignedURL(It.Is<GetPreSignedUrlRequest>(
                    y => y.BucketName == _bucketOptions.BucketName && y.Key == $"jobresults/{job.Id:D}")))
                .Returns(expectedPresignedUrl);

            var request = new JobResultsPreSignedUrlRequest(job.Id);
            var result = await _handler.Handle(request, CancellationToken.None);

            result.JobId.Should().Be(job.Id);
            result.GetUrl.Should().Be(expectedPresignedUrl);
        }

        [Fact]
        public void WithNonExistingJob_ThenThrowsApiException()
        {
            var request = new JobResultsPreSignedUrlRequest(Guid.NewGuid());
            var act = async () => await _handler.Handle(request, CancellationToken.None);

            act
                .Should()
                .ThrowAsync<ApiException>()
                .Result
                .Where(x =>
                    x.Message.Contains($"Upload job with id {request.JobId} not found.")
                    && x.StatusCode == StatusCodes.Status404NotFound);
        }

        [Theory]
        [InlineData(JobStatus.Created)]
        [InlineData(JobStatus.Cancelled)]
        [InlineData(JobStatus.Preparing)]
        [InlineData(JobStatus.Prepared)]
        [InlineData(JobStatus.Processing)]
        [InlineData(JobStatus.Error)]
        public void WithUncompletedJob_ThenThrowsApiException(JobStatus jobStatus)
        {
            var job = new Job(DateTimeOffset.Now, jobStatus) { Id = Guid.NewGuid() };
            _fakeBuildingGrbContext.Jobs.Add(job);
            _fakeBuildingGrbContext.SaveChanges();

            var request = new JobResultsPreSignedUrlRequest(job.Id);
            var act = async () => await _handler.Handle(request, CancellationToken.None);

            act
                .Should()
                .ThrowAsync<ApiException>()
                .Result
                .Where(x =>
                    x.Message.Contains($"Job with id {request.JobId} has not yet completed.")
                    && x.StatusCode == StatusCodes.Status400BadRequest);
        }
    }
}
