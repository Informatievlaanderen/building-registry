namespace BuildingRegistry.Tests.Grb.Handlers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Grb.Uploads;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.Api.Extract;
    using BuildingRegistry.Grb.Abstractions;
    using FluentAssertions;
    using Xunit;

    public class GivenGetJobResultRequest
    {
        private readonly Fixture _fixture;
        private readonly FakeBuildingGrbContext _fakeBuildingGrbContext;

        public GivenGetJobResultRequest()
        {
            _fixture = new Fixture();
            _fakeBuildingGrbContext = new FakeBuildingGrbContextFactory().CreateDbContext();
        }

        [Fact]
        public void WithNotExistingJobId_ThenReturnsNotFound()
        {
            var jobId = _fixture.Create<Guid>();
            var request = new GetJobResultRequest(jobId);
            var handler = new JobResultHandler(_fakeBuildingGrbContext);

            var act = () => handler.Handle(request, CancellationToken.None);

            act.Should().ThrowAsync<ApiException>()
                .WithMessage($"Upload job with id {jobId} not found or not completed.");
        }

        [Fact]
        public void WithNonCompleteJob_ThenReturnsNotFound()
        {
            // Arrange
            var job = _fixture.Create<Job>();
            job.Status = JobStatus.Prepared;
            _fakeBuildingGrbContext.Jobs.Add(job);

            var request = new GetJobResultRequest(job.Id);
            var handler = new JobResultHandler(_fakeBuildingGrbContext);

            // Act
            var act = () => handler.Handle(request, CancellationToken.None);

            // Assert
            act.Should().ThrowAsync<ApiException>()
                .WithMessage($"Upload job with id {job.Id} not found or not completed.");
        }

        [Fact]
        public async Task ThenReturnsExtractArchive()
        {
            // Arrange
            var job = _fixture.Create<Job>();
            job.Status = JobStatus.Completed;
            _fakeBuildingGrbContext.Jobs.Add(job);

            _fixture.Register(() => job.Id);
            var jobResults = _fixture.CreateMany<JobResult>(10);
            _fakeBuildingGrbContext.JobResults.AddRange(jobResults);

            var request = new GetJobResultRequest(job.Id);
            var handler = new JobResultHandler(_fakeBuildingGrbContext);

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            foreach (var r in result)
            {
                r.Should().NotBeNull();
                r.Should().BeOfType<ExtractFile>();

                var file = r as ExtractFile;
                file.Name.ToString().Should().NotBeNullOrEmpty();
                file.Name.ToString().Should().Be("IdnGrResults.dbf");
            }
        }
    }
}
