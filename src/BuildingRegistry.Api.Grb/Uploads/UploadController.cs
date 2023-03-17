namespace BuildingRegistry.Api.Grb.Uploads
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.AcmIdm;
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

        [HttpPost("job")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = PolicyNames.IngemetenGebouw.GrbBijwerker)]
        public async Task<IActionResult> GetPreSignedUrl(CancellationToken cancellationToken)
        {
            return Ok(await _mediator.Send(new GetPreSignedUrlRequest(), cancellationToken));
        }

        //TODO: GetResult (uploads/{id:guid})
    }
}
