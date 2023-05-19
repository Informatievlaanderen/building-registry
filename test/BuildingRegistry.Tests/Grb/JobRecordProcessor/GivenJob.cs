namespace BuildingRegistry.Tests.Grb.JobRecordProcessor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.BackOffice.Abstractions.Building.Requests;
    using Be.Vlaanderen.Basisregisters.BasicApiProblem;
    using BuildingRegistry.Grb.Abstractions;
    using BuildingRegistry.Grb.Processor.Job;
    using FluentAssertions;
    using Handlers;
    using Moq;
    using NetTopologySuite.Geometries;
    using Xunit;

    public class GivenJob
    {
        [Fact]
        public async Task ThenJobRecordsWithStatusCreatedAreProcessed()
        {
            var buildingGrbContext = new FakeBuildingGrbContextFactory().CreateDbContext();
            var backOfficeApiProxy = new Mock<IBackOfficeApiProxy>();

            var job = new Job(DateTimeOffset.Now, JobStatus.Prepared, Guid.NewGuid());
            await buildingGrbContext.Jobs.AddAsync(job);

            var jobRecordWithStatusCreated = CreateJobRecord(job.Id, jobRecordId: 1, JobRecordStatus.Created);
            var jobRecordWithStatusPending = CreateJobRecord(job.Id, jobRecordId: 2, JobRecordStatus.Pending);
            var jobRecordWithStatusWarning = CreateJobRecord(job.Id, jobRecordId: 3, JobRecordStatus.Warning);
            var jobRecordWithStatusError = CreateJobRecord(job.Id, jobRecordId: 4, JobRecordStatus.Error);
            var jobRecordWithStatusComplete = CreateJobRecord(job.Id, jobRecordId: 5, JobRecordStatus.Complete);

            await buildingGrbContext.JobRecords.AddAsync(jobRecordWithStatusCreated);
            await buildingGrbContext.JobRecords.AddAsync(jobRecordWithStatusPending);
            await buildingGrbContext.JobRecords.AddAsync(jobRecordWithStatusWarning);
            await buildingGrbContext.JobRecords.AddAsync(jobRecordWithStatusError);
            await buildingGrbContext.JobRecords.AddAsync(jobRecordWithStatusComplete);
            await buildingGrbContext.SaveChangesAsync();

            var ticketId = Guid.NewGuid();
            backOfficeApiProxy
                .Setup(x => x.ChangeBuildingMeasurement(
                    It.IsAny<int>(),
                    It.Is<ChangeBuildingMeasurementRequest>(y => y.GrbData.Idn == jobRecordWithStatusCreated.Idn),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BackOfficeApiResult($"https://ticketing.be/{ticketId}", new List<ValidationError>()));

            var jobRecordsProcessor = new JobRecordsProcessor(
                buildingGrbContext,
                backOfficeApiProxy.Object);

            //act
            await jobRecordsProcessor.Process(job.Id, CancellationToken.None);

            //assert
            backOfficeApiProxy.Verify(x => x.ChangeBuildingMeasurement(
                    It.IsAny<int>(),
                    It.Is<ChangeBuildingMeasurementRequest>(y => y.GrbData.Idn == jobRecordWithStatusCreated.Idn),
                    It.IsAny<CancellationToken>()),
                Times.Once);
            backOfficeApiProxy.Verify(x => x.ChangeBuildingMeasurement(
                    jobRecordWithStatusCreated.GrId,
                    It.Is<ChangeBuildingMeasurementRequest>(y => y.GrbData.Idn == jobRecordWithStatusCreated.Idn),
                    It.IsAny<CancellationToken>()),
                Times.Once);
            var jobRecordEntity = buildingGrbContext.JobRecords.Single(x => x.Id == jobRecordWithStatusCreated.Id);
            jobRecordEntity.Status.Should().Be(JobRecordStatus.Pending);
            jobRecordEntity.TicketId.Should().Be(ticketId);
        }

        private static JobRecord CreateJobRecord(Guid jobId, int jobRecordId, JobRecordStatus jobRecordStatus)
        {
            return new JobRecord
            {
                Id = jobRecordId,
                JobId = jobId,
                Status = jobRecordStatus,
                EventType = GrbEventType.ChangeBuildingMeasurement,
                Geometry = (Polygon)GeometryHelper.ValidPolygon,
                GrbObject = GrbObject.ArtWork,
                GrbObjectType = GrbObjectType.MainBuilding,
                GrId = 1,
                Idn = 3
            };
        }
    }
}
