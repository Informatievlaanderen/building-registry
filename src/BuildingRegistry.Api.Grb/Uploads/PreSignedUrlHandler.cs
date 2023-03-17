namespace BuildingRegistry.Api.Grb.Uploads
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Infrastructure;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.Extensions.Options;
    using TicketingService.Abstractions;

    public sealed record GetPreSignedUrlRequest : IRequest<GetPreSignedUrlResponse>;

    public sealed record GetPreSignedUrlResponse(Guid JobId, string PreSignedUrl, Dictionary<string, string> PreSignedUrlFormData, string TicketUrl);

    public sealed class PreSignedUrlHandler : IRequestHandler<GetPreSignedUrlRequest, GetPreSignedUrlResponse>
    {
        private readonly ITicketing _ticketing;
        private readonly ITicketingUrl _ticketingUrl;
        private readonly IAmazonS3Extended _s3Extended;
        private readonly BucketOptions _bucketOptions;

        public PreSignedUrlHandler(
            ITicketing ticketing,
            ITicketingUrl ticketingUrl,
            IAmazonS3Extended s3Extended,
            IOptions<BucketOptions> bucketOptions)
        {
            _ticketing = ticketing;
            _ticketingUrl = ticketingUrl;
            _s3Extended = s3Extended;
            _bucketOptions = bucketOptions.Value ?? throw new ArgumentNullException(nameof(bucketOptions));
        }

        public async Task<GetPreSignedUrlResponse> Handle(
            GetPreSignedUrlRequest request,
            CancellationToken cancellationToken)
        {
            var jobId = Guid.NewGuid();

            var preSignedUrl = _s3Extended.CreatePresignedPost(
                new CreatePresignedPostRequest(
                    _bucketOptions.BucketName,
                    jobId.ToString("D"),
                    new List<ExactMatchCondition>(),
                    TimeSpan.FromMinutes(_bucketOptions.UrlExpirationInMinutes)));

            var ticketId= await _ticketing.CreateTicket(
                new Dictionary<string, string>
                {
                    { "Registry", "BuildingRegistry" },
                    { "Action", "GrbUpload" },
                    { "UploadId", jobId.ToString("D") }
                },
                cancellationToken);

            var ticketUrl = _ticketingUrl.For(ticketId);
            return new GetPreSignedUrlResponse(jobId, preSignedUrl.Url.ToString(), preSignedUrl.Fields, ticketUrl.ToString());
        }
    }
}
