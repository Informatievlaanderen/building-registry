namespace BuildingRegistry.Tests.Grb.JobRecordProcessor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.BackOffice.Abstractions.Building;
    using Api.BackOffice.Abstractions.Building.Requests;
    using Be.Vlaanderen.Basisregisters.BasicApiProblem;
    using BuildingRegistry.Grb.Abstractions;
    using BuildingRegistry.Grb.Processor.Job;
    using FluentAssertions;
    using Handlers;
    using Moq;
    using NetTopologySuite.Geometries;
    using TicketingService.Abstractions;
    using Xunit;

    public class GivenDefineBuilding
    {
        public GivenDefineBuilding()
        {
        }

        [Fact]
        public async Task ThenRealizeAndMeasureUnplannedBuildingRequestIsSent()
        {
            var buildingGrbContext = new FakeBuildingGrbContextFactory().CreateDbContext();
            var ticketing = new Mock<ITicketing>();
            var backOfficeApiProxy = new Mock<IBackOfficeApiProxy>();

            var job = new Job(DateTimeOffset.Now, JobStatus.Prepared, Guid.NewGuid());
            await buildingGrbContext.Jobs.AddAsync(job);

            var jobRecord = new JobRecord
            {
                JobId = job.Id,
                Status = JobRecordStatus.Created,
                EventType = GrbEventType.DefineBuilding,
                Geometry = (Polygon)GeometryHelper.ValidPolygon,
                GrbObject = GrbObject.ArtWork,
                GrbObjectType = GrbObjectType.MainBuilding,
                GrId = 1,
                Id = 2,
                Idn = 3
            };

            await buildingGrbContext.JobRecords.AddAsync(jobRecord);
            await buildingGrbContext.SaveChangesAsync();

            var ticketId = Guid.NewGuid();
            backOfficeApiProxy
                .Setup(x => x.RealizeAndMeasureUnplannedBuilding(
                    It.Is<RealizeAndMeasureUnplannedBuildingRequest>(y => y.GrbData.Idn == jobRecord.Idn),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BackOfficeApiResult($"https://ticketing.be/{ticketId}", new List<ValidationError>()));

            var jobRecordsProcessor = new JobRecordsProcessor(
                buildingGrbContext,
                ticketing.Object,
                backOfficeApiProxy.Object);

            //act
            await jobRecordsProcessor.Process(new List<JobRecord> { jobRecord }, CancellationToken.None);

            //assert
            backOfficeApiProxy.Verify(x => x.RealizeAndMeasureUnplannedBuilding(
                    It.Is<RealizeAndMeasureUnplannedBuildingRequest>(y => y.GrbData.Idn == jobRecord.Idn),
                    It.IsAny<CancellationToken>()),
                Times.Once);
            var jobRecordEntity = buildingGrbContext.JobRecords.First(x => x.Id == jobRecord.Id);
            jobRecordEntity.Status.Should().Be(JobRecordStatus.Pending);
            jobRecordEntity.TicketId.Should().Be(ticketId);
        }
    }
}
