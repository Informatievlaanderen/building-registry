namespace BuildingRegistry.Tests.Grb.JobProcessor
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using BuildingRegistry.Grb.Abstractions;
    using BuildingRegistry.Grb.Processor.Job;
    using Handlers;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging.Abstractions;
    using Microsoft.Extensions.Options;
    using Moq;
    using TicketingService.Abstractions;
    using Xunit;

    public class GivenPreparedJob
    {
        [Fact]
        public async Task ThenProcessJob()
        {
            var buildingGrbContext = new FakeBuildingGrbContextFactory().CreateDbContext();

            var jobRecordsProcessor = new Mock<IJobRecordsProcessor>();
            var jobRecordsMonitor = new Mock<IJobRecordsMonitor>();
            var mockJobResultsUploader = new Mock<IJobResultUploader>();
            var mockJobRecordsArchiver = new Mock<IJobRecordsArchiver>();
            var hostApplicationLifetime = new Mock<IHostApplicationLifetime>();

            var jobProcessor = new JobProcessor(
                buildingGrbContext,
                jobRecordsProcessor.Object,
                jobRecordsMonitor.Object,
                mockJobResultsUploader.Object,
                mockJobRecordsArchiver.Object,
                Mock.Of<ITicketing>(),
                new OptionsWrapper<GrbApiOptions>(new GrbApiOptions { GrbApiUrl = "https://api-vlaanderen.be/gebouwen/uploads"}),
                hostApplicationLifetime.Object,
                new NullLoggerFactory());

            var job = new Job(DateTimeOffset.Now.AddMinutes(-10), JobStatus.Prepared, ticketId: Guid.NewGuid());
            var entityEntry = buildingGrbContext.Jobs.Add(job);
            await buildingGrbContext.SaveChangesAsync();

            //act
            await jobProcessor.StartAsync(CancellationToken.None);
            await jobProcessor.ExecuteTask;

            //assert
            jobRecordsProcessor.Verify(x => x.Process(It.IsAny<IEnumerable<JobRecord>>(), It.IsAny<CancellationToken>()), Times.Once);
            jobRecordsMonitor.Verify(x => x.Monitor(It.IsAny<IEnumerable<JobRecord>>(), It.IsAny<CancellationToken>()), Times.Once);
            mockJobResultsUploader.Verify(x => x.UploadJob(job, It.IsAny<CancellationToken>()));
            mockJobRecordsArchiver.Verify(x => x.Archive(job.Id));
            hostApplicationLifetime.Verify(x => x.StopApplication(), Times.Once);
        }
    }
}
