namespace BuildingRegistry.Api.Grb.Uploads
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Infrastructure;
    using MediatR;
    using TicketingService.Abstractions;

    public sealed record GetPreSignedUrlRequest : IRequest<GetPreSignedUrlResponse>;

    public sealed record GetPreSignedUrlResponse(Guid JobId, string PreSignedUrl, string TicketUrl);

    public sealed class PreSignedUrlHandler : IRequestHandler<GetPreSignedUrlRequest, GetPreSignedUrlResponse>
    {
        private readonly ITicketing _ticketing;
        private readonly ITicketingUrl _ticketingUrl;
        private readonly IAmazonS3Extended _s3Extended;

        public PreSignedUrlHandler(
            ITicketing ticketing,
            ITicketingUrl ticketingUrl,
            IAmazonS3Extended s3Extended)
        {
            _ticketing = ticketing;
            _ticketingUrl = ticketingUrl;
            _s3Extended = s3Extended;
        }

        public async Task<GetPreSignedUrlResponse> Handle(
            GetPreSignedUrlRequest request,
            CancellationToken cancellationToken)
        {
            var jobId = Guid.NewGuid();

            var preSignedUrl = _s3Extended.CreatePresignedPost(
                new CreatePresignedPostRequest(
                    "basisregisters-staging-building-grb-uploads",
                    jobId.ToString("D"),
                    new List<ExactMatchCondition>(),
                    TimeSpan.FromHours(1))); //config?

            var ticketId= await _ticketing.CreateTicket(
                new Dictionary<string, string>
                {
                    { "Registry", "BuildingRegistry" },
                    { "Action", "GrbUpload" },
                    { "UploadId", jobId.ToString("D") }
                },
                cancellationToken);

            var ticketUrl = _ticketingUrl.For(ticketId);
            return new GetPreSignedUrlResponse(jobId, preSignedUrl.Url.ToString(), ticketUrl.ToString());
        }
    }
}
