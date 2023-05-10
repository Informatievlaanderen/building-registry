namespace BuildingRegistry.Tests.Grb.JobProcessor
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using BuildingRegistry.Grb.Abstractions;
    using BuildingRegistry.Grb.Processor.Job;
    using BuildingRegistry.Tests.Grb.Handlers;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging.Abstractions;
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
            var hostApplicationLifetime = new Mock<IHostApplicationLifetime>();

            var jobProcessor = new JobProcessor(
                buildingGrbContext,
                jobRecordsProcessor.Object,
                jobRecordsMonitor.Object,
                Mock.Of<ITicketing>(), hostApplicationLifetime.Object, new NullLoggerFactory());

            buildingGrbContext.Jobs.Add(new Job(DateTimeOffset.Now.AddMinutes(-10), JobStatus.Prepared));
            await buildingGrbContext.SaveChangesAsync();

            //act
            await jobProcessor.StartAsync(CancellationToken.None);
            await jobProcessor.ExecuteTask;

            //assert
            jobRecordsProcessor.Verify(x => x.Process(It.IsAny<IEnumerable<JobRecord>>(), It.IsAny<CancellationToken>()), Times.Once);
            jobRecordsMonitor.Verify(x => x.Monitor(It.IsAny<IEnumerable<JobRecord>>(), It.IsAny<CancellationToken>()), Times.Once);
            hostApplicationLifetime.Verify(x => x.StopApplication(), Times.Once);
        }
    }
}
