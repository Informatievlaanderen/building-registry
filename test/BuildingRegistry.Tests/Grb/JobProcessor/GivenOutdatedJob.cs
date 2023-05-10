namespace BuildingRegistry.Tests.Grb.JobProcessor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BuildingRegistry.Grb.Abstractions;
    using BuildingRegistry.Grb.Processor.Job;
    using BuildingRegistry.Tests.Grb.Handlers;
    using FluentAssertions;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging.Abstractions;
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
            var hostApplicationLifetime = new Mock<IHostApplicationLifetime>();

            var jobProcessor = new JobProcessor(
                buildingGrbContext,
                jobRecordsProcessor.Object,
                jobRecordsMonitor.Object,
                Mock.Of<ITicketing>(), hostApplicationLifetime.Object, new NullLoggerFactory());

            const int maxLifeTimeJob = 65;
            var expiredDateTime = DateTimeOffset.Now.AddMinutes(-1 * (maxLifeTimeJob + 1));
            var job = new Job(expiredDateTime, JobStatus.Created);
            buildingGrbContext.Jobs.Add(job);
            await buildingGrbContext.SaveChangesAsync();

            //act
            await jobProcessor.StartAsync(CancellationToken.None);
            await jobProcessor.ExecuteTask;

            //assert
            buildingGrbContext.Jobs.All(x => x.Status == JobStatus.Cancelled).Should().BeTrue();
            jobRecordsProcessor.Verify(x => x.Process(It.IsAny<IEnumerable<JobRecord>>(), It.IsAny<CancellationToken>()), Times.Never);
            jobRecordsMonitor.Verify(x => x.Monitor(It.IsAny<IEnumerable<JobRecord>>(), It.IsAny<CancellationToken>()), Times.Never);
            job.Status.Should().Be(JobStatus.Cancelled);
            job.LastChanged.Should().BeAfter(job.Created);
            hostApplicationLifetime.Verify(x => x.StopApplication(), Times.Once);
        }
    }
}
