namespace BuildingRegistry.Tests.Grb.JobProcessor
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BuildingRegistry.Grb.Abstractions;
    using BuildingRegistry.Grb.Processor.Job;
    using FluentAssertions;
    using Handlers;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging.Abstractions;
    using Microsoft.Extensions.Options;
    using Moq;
    using TicketingService.Abstractions;
    using Xunit;

    public class GivenOutdatedJob
    {
        [Fact]
        public async Task ThenJobIsCanceled()
        {
            var buildingGrbContext = new FakeBuildingGrbContextFactory().CreateDbContext();

            var jobRecordsProcessor = new Mock<IJobRecordsProcessor>();
            var jobRecordsMonitor = new Mock<IJobRecordsMonitor>();
            var jobResultsUploader = new Mock<IJobResultUploader>();
            var jobRecordsArchiver = new Mock<IJobRecordsArchiver>();
            var ticketing = new Mock<ITicketing>();
            var hostApplicationLifetime = new Mock<IHostApplicationLifetime>();

            var jobProcessor = new JobProcessor(
                buildingGrbContext,
                jobRecordsProcessor.Object,
                jobRecordsMonitor.Object,
                jobResultsUploader.Object,
                jobRecordsArchiver.Object,
                ticketing.Object,
                new OptionsWrapper<GrbApiOptions>(new GrbApiOptions { PublicApiUrl = "https://api-vlaanderen.be/gebouwen/uploads"}),
                hostApplicationLifetime.Object,
                new NullLoggerFactory());

            const int maxLifeTimeJob = 65;
            var expiredDateTime = DateTimeOffset.Now.AddMinutes(-1 * (maxLifeTimeJob + 1));
            var job = new Job(expiredDateTime, JobStatus.Created) { TicketId = Guid.NewGuid() } ;
            buildingGrbContext.Jobs.Add(job);
            await buildingGrbContext.SaveChangesAsync();

            //act
            await jobProcessor.StartAsync(CancellationToken.None);
            await jobProcessor.ExecuteTask;

            //assert
            buildingGrbContext.Jobs.All(x => x.Status == JobStatus.Cancelled).Should().BeTrue();
            jobRecordsProcessor.Verify(x => x.Process(job.Id, It.IsAny<CancellationToken>()), Times.Never);
            jobRecordsMonitor.Verify(x => x.Monitor(job.Id, It.IsAny<CancellationToken>()), Times.Never);
            jobResultsUploader.Verify(x => x.UploadJobResults(job.Id, It.IsAny<CancellationToken>()), Times.Never);
            jobRecordsArchiver.Verify(x => x.Archive(job.Id, It.IsAny<CancellationToken>()), Times.Never);
            job.Status.Should().Be(JobStatus.Cancelled);
            job.LastChanged.Should().BeAfter(job.Created);
            ticketing.Verify(x => x.Complete(
                job.TicketId!.Value,
                new TicketResult(new { JobStatus = "Cancelled" }),
                It.IsAny<CancellationToken>()));
            hostApplicationLifetime.Verify(x => x.StopApplication(), Times.Once);
        }
    }
}
