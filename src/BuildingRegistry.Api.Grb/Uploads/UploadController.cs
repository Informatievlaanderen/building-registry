namespace BuildingRegistry.Api.Grb.Uploads
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
    using Be.Vlaanderen.Basisregisters.Api;
    using MediatR;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion("2.0")]
    [AdvertiseApiVersions("2.0")]
    [ApiRoute("uploads")]
    [ApiExplorerSettings(GroupName = "Upload")]
    public class UploadController : ApiController
    {
        private readonly IMediator _mediator;

        public UploadController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("jobs")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = PolicyNames.IngemetenGebouw.GrbBijwerker)]
        public async Task<IActionResult> GetUploadPreSignedUrl(CancellationToken cancellationToken)
        {
            return Ok(await _mediator.Send(new UploadPreSignedUrlRequest(), cancellationToken));
        }

        [HttpGet("jobs/{jobId:guid}/results")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = PolicyNames.IngemetenGebouw.GrbBijwerker)]
        public async Task<IActionResult> GetResultsPreSignedUrl(Guid jobId, CancellationToken cancellationToken)
        {
            return Ok(await _mediator.Send(new JobResultsPreSignedUrlRequest(jobId), cancellationToken));
        }

        [HttpDelete("jobs/{jobId:guid}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = PolicyNames.IngemetenGebouw.GrbBijwerker)]
        public async Task<IActionResult> CancelJob(Guid jobId, CancellationToken cancellationToken)
        {
            await _mediator.Send(new CancelJobRequest(jobId), cancellationToken);
            return NoContent();
        }
    }
}
