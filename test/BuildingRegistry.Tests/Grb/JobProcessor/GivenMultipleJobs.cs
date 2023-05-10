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
    using NetTopologySuite.Geometries;
    using TicketingService.Abstractions;
    using Xunit;

    public class GivenMultipleJobs
    {
        [Fact]
        public async Task WithFirstJobNotPrepared_ThenNothing()
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

            buildingGrbContext.Jobs.Add(new Job(DateTimeOffset.Now.AddMinutes(-10), JobStatus.Created));
            buildingGrbContext.Jobs.Add(new Job(DateTimeOffset.Now.AddMinutes(-9), JobStatus.Prepared));
            await buildingGrbContext.SaveChangesAsync();

            await jobProcessor.StartAsync(CancellationToken.None);
            await jobProcessor.ExecuteTask;

            jobRecordsProcessor.Verify(x => x.Process(It.IsAny<IEnumerable<JobRecord>>(), It.IsAny<CancellationToken>()), Times.Never);
            jobRecordsMonitor.Verify(x => x.Monitor(It.IsAny<IEnumerable<JobRecord>>(), It.IsAny<CancellationToken>()), Times.Never);
            hostApplicationLifetime.Verify(x => x.StopApplication(), Times.Once);
        }

        [Fact]
        public async Task WithOnlyFirstJobPrepared_ThenProcessFirstJobOnly()
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

            var firstJob = new Job(DateTimeOffset.Now.AddMinutes(-10), JobStatus.Prepared) { Id = Guid.NewGuid() };
            var secondJob = new Job(DateTimeOffset.Now.AddMinutes(-9), JobStatus.Created) { Id = Guid.NewGuid() };
            buildingGrbContext.Jobs.Add(firstJob);
            buildingGrbContext.Jobs.Add(secondJob);

            var jobRecordOfFirstJob = CreateJobRecord(firstJob.Id, 1);
            var jobRecordOfSecondJob = CreateJobRecord(secondJob.Id, 2);
            buildingGrbContext.JobRecords.Add(jobRecordOfFirstJob);
            buildingGrbContext.JobRecords.Add(jobRecordOfSecondJob);

            await buildingGrbContext.SaveChangesAsync();

            //act
            await jobProcessor.StartAsync(CancellationToken.None);
            await jobProcessor.ExecuteTask;

            //assert
            jobRecordsProcessor.Verify(x => x.Process(It.Is<IEnumerable<JobRecord>>(x => x.First() == jobRecordOfFirstJob), It.IsAny<CancellationToken>()), Times.Once);
            jobRecordsProcessor.Verify(x => x.Process(It.Is<IEnumerable<JobRecord>>(x => x.First() == jobRecordOfSecondJob), It.IsAny<CancellationToken>()), Times.Never);
            firstJob.Status.Should().Be(JobStatus.Completed);
            firstJob.LastChanged.Should().BeAfter(firstJob.Created);
            jobRecordsMonitor.Verify(x => x.Monitor(It.Is<IEnumerable<JobRecord>>(x => x.First() == jobRecordOfFirstJob), It.IsAny<CancellationToken>()), Times.Once);
            jobRecordsMonitor.Verify(x => x.Monitor(It.Is<IEnumerable<JobRecord>>(x => x.First() == jobRecordOfSecondJob), It.IsAny<CancellationToken>()), Times.Never);
            hostApplicationLifetime.Verify(x => x.StopApplication(), Times.Once);
        }

        [Fact]
        public async Task WithAllJobsPrepared_ThenProcessAllJobsInOrder()
        {
            var buildingGrbContext = new FakeBuildingGrbContextFactory().CreateDbContext();

            var jobRecordExecutionSequence = new List<Guid>();
            var jobRecordsProcessor = new Mock<IJobRecordsProcessor>();
            jobRecordsProcessor
                .Setup(x => x.Process(It.IsAny<IEnumerable<JobRecord>>(), It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<JobRecord>, CancellationToken>((jobRecords, _) => jobRecordExecutionSequence.Add(jobRecords.First().JobId));
            var jobRecordsMonitor = new Mock<IJobRecordsMonitor>();
            var hostApplicationLifetime = new Mock<IHostApplicationLifetime>();

            var jobProcessor = new JobProcessor(
                buildingGrbContext,
                jobRecordsProcessor.Object,
                jobRecordsMonitor.Object,
                Mock.Of<ITicketing>(), hostApplicationLifetime.Object, new NullLoggerFactory());

            var firstJob = new Job(DateTimeOffset.Now.AddMinutes(-10), JobStatus.Prepared) { Id = Guid.NewGuid() };
            var secondJob = new Job(DateTimeOffset.Now.AddMinutes(-9), JobStatus.Prepared) { Id = Guid.NewGuid() };
            buildingGrbContext.Jobs.Add(firstJob);
            buildingGrbContext.Jobs.Add(secondJob);

            var jobRecordOfFirstJob = CreateJobRecord(firstJob.Id, 1);
            var jobRecordOfSecondJob = CreateJobRecord(secondJob.Id, 2);
            buildingGrbContext.JobRecords.Add(jobRecordOfFirstJob);
            buildingGrbContext.JobRecords.Add(jobRecordOfSecondJob);

            await buildingGrbContext.SaveChangesAsync();

            //act
            await jobProcessor.StartAsync(CancellationToken.None);
            await jobProcessor.ExecuteTask;

            //assert
            jobRecordsProcessor.Verify(x => x.Process(It.Is<IEnumerable<JobRecord>>(x => x.First() == jobRecordOfFirstJob), It.IsAny<CancellationToken>()), Times.Once);
            jobRecordsProcessor.Verify(x => x.Process(It.Is<IEnumerable<JobRecord>>(x => x.First() == jobRecordOfSecondJob), It.IsAny<CancellationToken>()), Times.Once);
            jobRecordExecutionSequence.First().Should().Be(firstJob.Id);
            jobRecordExecutionSequence.Last().Should().Be(secondJob.Id);
            firstJob.Status.Should().Be(JobStatus.Completed);
            firstJob.LastChanged.Should().BeAfter(firstJob.Created);
            secondJob.Status.Should().Be(JobStatus.Completed);
            secondJob.LastChanged.Should().BeAfter(firstJob.Created);
            jobRecordsMonitor.Verify(x => x.Monitor(It.Is<IEnumerable<JobRecord>>(x => x.First() == jobRecordOfFirstJob), It.IsAny<CancellationToken>()), Times.Once);
            jobRecordsMonitor.Verify(x => x.Monitor(It.Is<IEnumerable<JobRecord>>(x => x.First() == jobRecordOfSecondJob), It.IsAny<CancellationToken>()), Times.Once);
            hostApplicationLifetime.Verify(x => x.StopApplication(), Times.Once);
        }

        private JobRecord CreateJobRecord(Guid jobId, int id)
        {
            return new JobRecord
            {
                JobId = jobId,
                Status = JobRecordStatus.Created,
                EventType = GrbEventType.DefineBuilding,
                Geometry = (Polygon)GeometryHelper.ValidPolygon,
                GrbObject = GrbObject.ArtWork,
                GrbObjectType = GrbObjectType.MainBuilding,
                GrId = 1,
                Id = id,
                Idn = 3
            };
        }
    }
}
