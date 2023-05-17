namespace BuildingRegistry.Api.Grb.Uploads
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using BuildingRegistry.Grb.Abstractions;
    using Infrastructure;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.Extensions.Options;
    using NodaTime;
    using TicketingService.Abstractions;

    public sealed record UploadPreSignedUrlRequest : IRequest<UploadPreSignedUrlResponse>;

    public sealed record UploadPreSignedUrlResponse(Guid JobId, string UploadUrl, Dictionary<string, string> UploadUrlFormData, string TicketUrl);

    public sealed class UploadPreSignedUrlHandler : IRequestHandler<UploadPreSignedUrlRequest, UploadPreSignedUrlResponse>
    {
        private readonly BuildingGrbContext _buildingGrbContext;
        private readonly ITicketing _ticketing;
        private readonly ITicketingUrl _ticketingUrl;
        private readonly IAmazonS3Extended _s3Extended;
        private readonly BucketOptions _bucketOptions;

        public UploadPreSignedUrlHandler(
            BuildingGrbContext buildingGrbContext,
            ITicketing ticketing,
            ITicketingUrl ticketingUrl,
            IAmazonS3Extended s3Extended,
            IOptions<BucketOptions> bucketOptions)
        {
            _buildingGrbContext = buildingGrbContext;
            _ticketing = ticketing;
            _ticketingUrl = ticketingUrl;
            _s3Extended = s3Extended;
            _bucketOptions = bucketOptions.Value ?? throw new ArgumentNullException(nameof(bucketOptions));
        }

        public async Task<UploadPreSignedUrlResponse> Handle(
            UploadPreSignedUrlRequest request,
            CancellationToken cancellationToken)
        {
            await using var transaction = await _buildingGrbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var job = await CreateJob(cancellationToken);

                var preSignedUrl = _s3Extended.CreatePresignedPost(
                    new CreatePresignedPostRequest(
                        _bucketOptions.BucketName,
                        job.BlobName,
                        new List<ExactMatchCondition>(),
                        TimeSpan.FromMinutes(_bucketOptions.UrlExpirationInMinutes)));

                var ticketId= await _ticketing.CreateTicket(
                    new Dictionary<string, string>
                    {
                        { "Registry", "BuildingRegistry" },
                        { "Action", "GrbUpload" },
                        { "UploadId", job.Id.ToString("D") }
                    },
                    cancellationToken);

                var ticketUrl = _ticketingUrl.For(ticketId).ToString();

                await UpdateJobWithTicketUrl(job, ticketId, cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return new UploadPreSignedUrlResponse(job.Id, preSignedUrl.Url.ToString(), preSignedUrl.Fields, ticketUrl);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        private async Task<Job> CreateJob(CancellationToken cancellationToken)
        {
            var job = new Job(
                SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                JobStatus.Created);

            await _buildingGrbContext.Jobs.AddAsync(job, cancellationToken);
            await _buildingGrbContext.SaveChangesAsync(cancellationToken);

            return job;
        }

        private async Task UpdateJobWithTicketUrl(Job job, Guid ticketId, CancellationToken cancellationToken)
        {
            job.TicketId = ticketId;
            await _buildingGrbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
