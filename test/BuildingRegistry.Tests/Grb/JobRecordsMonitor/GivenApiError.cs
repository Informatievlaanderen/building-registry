namespace BuildingRegistry.Tests.Grb.JobRecordsMonitor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BuildingRegistry.Grb.Abstractions;
    using BuildingRegistry.Grb.Processor.Job;
    using FluentAssertions;
    using Handlers;
    using Moq;
    using NetTopologySuite.Geometries;
    using TicketingService.Abstractions;
    using Xunit;

    public class GivenJobRecordsInError
    {
        [Fact]
        public async Task ThenJobRecordIsInError()
        {
            var buildingGrbContext = new FakeBuildingGrbContextFactory().CreateDbContext();
            var ticketing = new Mock<ITicketing>();

            var job = new Job(DateTimeOffset.Now, JobStatus.Prepared, Guid.NewGuid());
            await buildingGrbContext.Jobs.AddAsync(job);
            var jobRecord = CreateJobRecord(job.Id, 1);
            await buildingGrbContext.JobRecords.AddAsync(jobRecord);
            await buildingGrbContext.SaveChangesAsync();

            ticketing
                .Setup(x => x.Get(jobRecord.TicketId!.Value, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Ticket(jobRecord.TicketId!.Value, TicketStatus.Error,
                    new Dictionary<string, string>(),
                    new TicketResult(new TicketError("message", "code"))));

            var monitor = new JobRecordsMonitor(buildingGrbContext, ticketing.Object);

            //act
            await monitor.Monitor(job.Id, CancellationToken.None);

            //assert
            var jobRecordEntity = buildingGrbContext.JobRecords.First(x => x.Id == jobRecord.Id);
            jobRecordEntity.Status.Should().Be(JobRecordStatus.Error);
            jobRecordEntity.ErrorMessage.Should().Be("message");
        }

        [Fact]
        public async Task ThenJobRecordIsInWarning()
        {
            var buildingGrbContext = new FakeBuildingGrbContextFactory().CreateDbContext();
            var ticketing = new Mock<ITicketing>();

            var job = new Job(DateTimeOffset.Now, JobStatus.Prepared, Guid.NewGuid());
            await buildingGrbContext.Jobs.AddAsync(job);
            var jobRecord = CreateJobRecord(job.Id, 1);
            await buildingGrbContext.JobRecords.AddAsync(jobRecord);
            await buildingGrbContext.SaveChangesAsync();

            ticketing
                .Setup(x => x.Get(jobRecord.TicketId!.Value, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Ticket(jobRecord.TicketId!.Value, TicketStatus.Error,
                    new Dictionary<string, string>(),
                    new TicketResult(new TicketError("message", "VerwijderdGebouw"))));

            var monitor = new JobRecordsMonitor(buildingGrbContext, ticketing.Object);

            //act
            await monitor.Monitor(job.Id, CancellationToken.None);

            //assert
            var jobRecordEntity = buildingGrbContext.JobRecords.First(x => x.Id == jobRecord.Id);
            jobRecordEntity.Status.Should().Be(JobRecordStatus.Warning);
            jobRecordEntity.ErrorMessage.Should().Be("message");
        }


        private JobRecord CreateJobRecord(Guid jobId, int id)
        {
            return new JobRecord
            {
                Id = id,
                JobId = jobId,
                Status = JobRecordStatus.Pending,
                EventType = GrbEventType.DefineBuilding,
                Geometry = (Polygon) GeometryHelper.ValidPolygon,
                GrbObject = GrbObject.ArtWork,
                GrbObjectType = GrbObjectType.MainBuilding,
                GrId = 1,
                Idn = 3,
                TicketId = Guid.NewGuid()
            };
        }
    }
}
