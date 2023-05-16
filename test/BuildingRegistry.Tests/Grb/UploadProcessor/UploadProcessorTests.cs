namespace BuildingRegistry.Tests.Grb.UploadProcessor
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using Amazon.ECS;
    using Amazon.ECS.Model;
    using Be.Vlaanderen.Basisregisters.BlobStore;
    using BuildingRegistry.Grb.Abstractions;
    using BuildingRegistry.Grb.Processor.Upload;
    using BuildingRegistry.Grb.Processor.Upload.Zip;
    using Handlers;
    using FluentAssertions;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging.Abstractions;
    using Microsoft.Extensions.Options;
    using Moq;
    using TicketingService.Abstractions;
    using Xunit;
    using Task = System.Threading.Tasks.Task;

    public class UploadProcessorTests
    {
        private readonly FakeBuildingGrbContext _buildingGrbContext;

        public UploadProcessorTests()
        {
            _buildingGrbContext = new FakeBuildingGrbContextFactory().CreateDbContext();
        }

        [Fact]
        public async Task FlowTest()
        {
            var mockTicketing = new Mock<ITicketing>();
            var mockIBlobClient = new Mock<IBlobClient>();
            var mockAmazonClient = new Mock<IAmazonECS>();
            var mockIHostApplicationLifeTime = new Mock<IHostApplicationLifetime>();

            var ticketId = Guid.NewGuid();
            var job = new Job(DateTimeOffset.Now, JobStatus.Created, ticketId);

            _buildingGrbContext.Jobs.Add(job);
            await _buildingGrbContext.SaveChangesAsync(CancellationToken.None);

            var blobName = new BlobName("received/" + job.BlobName);

            mockIBlobClient
                .Setup(x => x.BlobExistsAsync(blobName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            mockIBlobClient
                .Setup(x => x.GetBlobAsync(blobName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BlobObject(
                    blobName,
                    null,
                    ContentType.Parse("X-multipart/abc"),
                    _  => Task.FromResult((Stream)new FileStream($"{AppContext.BaseDirectory}/Grb/UploadProcessor/gebouw_ALL.zip", FileMode.Open, FileAccess.Read))));

            mockAmazonClient
                .Setup(x => x.StartTaskAsync(It.IsAny<StartTaskRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new StartTaskResponse());

            var sut = new UploadProcessor(
                _buildingGrbContext,
                mockTicketing.Object,
                mockIBlobClient.Object,
                mockAmazonClient.Object,
                new NullLoggerFactory(),
                mockIHostApplicationLifeTime.Object,
                Options.Create(new EcsTaskOptions()));

            // Act
            await sut.StartAsync(CancellationToken.None);

            // Assert
            mockTicketing.Verify(x => x.Pending(ticketId, It.IsAny<CancellationToken>()), Times.Once );

            var jobRecords = _buildingGrbContext.JobRecords.Where(x => x.JobId == job.Id);
            jobRecords.Should().HaveCount(10);
            _buildingGrbContext.Jobs.First().Status.Should().Be(JobStatus.Prepared);

            mockAmazonClient.Verify(x => x.StartTaskAsync(It.IsAny<StartTaskRequest>(), It.IsAny<CancellationToken>()), Times.Once);
            mockIHostApplicationLifeTime.Verify(x => x.StopApplication(), Times.Once);
        }

         [Fact]
        public async Task WhenBlobNotFound()
        {
            var mockTicketing = new Mock<ITicketing>();
            var mockIBlobClient = new Mock<IBlobClient>();
            var mockAmazonClient = new Mock<IAmazonECS>();
            var mockIHostApplicationLifeTime = new Mock<IHostApplicationLifetime>();

            var ticketId = Guid.NewGuid();
            var job = new Job(DateTimeOffset.Now, JobStatus.Created, ticketId);

            _buildingGrbContext.Jobs.Add(job);
            await _buildingGrbContext.SaveChangesAsync(CancellationToken.None);

            var blobName = new BlobName("received/" + job.BlobName);

            mockIBlobClient
                .Setup(x => x.BlobExistsAsync(blobName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var sut = new UploadProcessor(
                _buildingGrbContext,
                mockTicketing.Object,
                mockIBlobClient.Object,
                mockAmazonClient.Object,
                new NullLoggerFactory(),
                mockIHostApplicationLifeTime.Object,
                Options.Create(new EcsTaskOptions()));

            // Act
            await sut.StartAsync(CancellationToken.None);

            // Assert
            mockTicketing.Verify(x => x.Pending(ticketId, It.IsAny<CancellationToken>()), Times.Never);
        }


        [Fact]
        public async Task WhenBlobObjectIsNull_ThenLogErrorAndContinue()
        {
            var ct = CancellationToken.None;
            var mockTicketing = new Mock<ITicketing>();
            var mockIBlobClient = new Mock<IBlobClient>();
            var mockAmazonClient = new Mock<IAmazonECS>();
            var mockIHostApplicationLifeTime = new Mock<IHostApplicationLifetime>();

            var ticketId = Guid.NewGuid();
            var job = new Job(DateTimeOffset.Now, JobStatus.Created, ticketId);

            _buildingGrbContext.Jobs.Add(job);
            await _buildingGrbContext.SaveChangesAsync(ct);

            var blobName = new BlobName("received/" + job.BlobName);

            mockIBlobClient
                .Setup(x => x.BlobExistsAsync(blobName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            mockIBlobClient
                .Setup(x => x.GetBlobAsync(blobName, It.IsAny<CancellationToken>()))
                .ReturnsAsync((BlobObject?)null);

            var sut = new UploadProcessor(
                _buildingGrbContext,
                mockTicketing.Object,
                mockIBlobClient.Object,
                mockAmazonClient.Object,
                new NullLoggerFactory(),
                mockIHostApplicationLifeTime.Object,
                Options.Create(new EcsTaskOptions()));

            // Act
            await sut.StartAsync(ct);

            // Assert
            var jobRecords = _buildingGrbContext.JobRecords.Where(x => x.JobId == job.Id);
            jobRecords.Should().HaveCount(0);
        }

        [Fact]
        public async Task WhenZipArchiveValidationProblems_ThenTicketError()
        {
            var ct = CancellationToken.None;
            var mockTicketing = new Mock<ITicketing>();
            var mockIBlobClient = new Mock<IBlobClient>();
            var mockAmazonClient = new Mock<IAmazonECS>();
            var mockIHostApplicationLifeTime = new Mock<IHostApplicationLifetime>();

            var ticketId = Guid.NewGuid();
            var job = new Job(DateTimeOffset.Now, JobStatus.Created, ticketId);

            _buildingGrbContext.Jobs.Add(job);
            await _buildingGrbContext.SaveChangesAsync(ct);

            var blobName = new BlobName("received/" + job.BlobName);

            mockIBlobClient
                .Setup(x => x.BlobExistsAsync(blobName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var zipFileStream = new FileStream($"{AppContext.BaseDirectory}/Grb/UploadProcessor/gebouw_dbf_missing.zip", FileMode.Open, FileAccess.Read);

            mockIBlobClient
                .Setup(x => x.GetBlobAsync(blobName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BlobObject(blobName, null, ContentType.Parse("X-multipart/abc"), _ => Task.FromResult((Stream)zipFileStream)));

            var sut = new UploadProcessor(
                _buildingGrbContext,
                mockTicketing.Object,
                mockIBlobClient.Object,
                mockAmazonClient.Object,
                new NullLoggerFactory(),
                mockIHostApplicationLifeTime.Object,
                Options.Create(new EcsTaskOptions()));

            // Act
            await sut.StartAsync(ct);

            // Assert
            mockTicketing.Verify(x => x.Error(
                ticketId,
                It.Is<TicketError>(x => x.Errors.First().ErrorCode == "RequiredFileMissing" && x.Errors.First().ErrorMessage == ZipArchiveConstants.DBF_FILENAME.ToUpper()),
                It.IsAny<CancellationToken>()), Times.Once );

            var jobRecords = _buildingGrbContext.JobRecords.Where(x => x.JobId == job.Id);
            jobRecords.Should().HaveCount(0);

            _buildingGrbContext.Jobs.First().Status.Should().Be(JobStatus.Error);
        }

        [Fact]
        public async Task WhenBlobNotFoundException_ThenLogAndContinue()
        {
            var ct = CancellationToken.None;
            var mockTicketing = new Mock<ITicketing>();
            var mockIBlobClient = new Mock<IBlobClient>();
            var mockAmazonClient = new Mock<IAmazonECS>();
            var mockIHostApplicationLifeTime = new Mock<IHostApplicationLifetime>();

            var ticketId = Guid.NewGuid();
            var job = new Job(DateTimeOffset.Now, JobStatus.Created, ticketId);

            _buildingGrbContext.Jobs.Add(job);
            await _buildingGrbContext.SaveChangesAsync(ct);

            var blobName = new BlobName("received/" + job.BlobName);

            mockIBlobClient
                .Setup(x => x.BlobExistsAsync(blobName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            mockIBlobClient
                .Setup(x => x.GetBlobAsync(blobName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BlobObject(blobName, null, ContentType.Parse("X-multipart/abc"), _ => throw new BlobNotFoundException(blobName)));

            var sut = new UploadProcessor(
                _buildingGrbContext,
                mockTicketing.Object,
                mockIBlobClient.Object,
                mockAmazonClient.Object,
                new NullLoggerFactory(),
                mockIHostApplicationLifeTime.Object,
                Options.Create(new EcsTaskOptions()));

            // Act
            await sut.StartAsync(ct);

            // Assert
            var jobRecords = _buildingGrbContext.JobRecords.Where(x => x.JobId == job.Id);
            jobRecords.Should().HaveCount(0);
        }
    }
}
