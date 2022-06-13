namespace BuildingRegistry.Api.CrabImport.CrabImport
{
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Api;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing.CrabImport;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.Filters;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Post;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing.Api.Messages;
    using Handlers;
    using MediatR;
    using ApiController = Infrastructure.ApiController;

    [ApiVersion("1.0")]
    [AdvertiseApiVersions("1.0")]
    [ApiRoute("crabimport")]
    [ApiExplorerSettings(GroupName = "CRAB Import")]
    public class CrabImportController : ApiController
    {
        private readonly IMediator _mediator;

        public CrabImportController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Import een CRAB item.
        /// </summary>
        /// <param name="bus"></param>
        /// <param name="registerCrabImportList"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="202">Als het verzoek aanvaard is.</response>
        /// <response code="400">Als het verzoek ongeldige data bevat.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(void), StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerRequestExample(typeof(RegisterCrabImportRequest), typeof(RegisterCrabImportRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status202Accepted, typeof(RegisterCrabImportResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        [RequestSizeLimit(104_857_600)]
        public async Task<IActionResult> Post(
            [FromServices] IdempotentCommandHandlerModule bus,
            [FromBody] List<RegisterCrabImportRequest[]> registerCrabImportList,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tags = await _mediator.Send(new PostRequest(registerCrabImportList, GetMetadata(), bus), cancellationToken);
            return Accepted(tags.Any() ? tags.Max() : null);
        }

        [HttpGet("batch/{feed}")]
        public IActionResult GetBatchStatus(
            [FromServices] CrabImportContext context,
            [FromRoute] string feed)
        {
            var status = context.LastBatchFor((ImportFeed)feed);
            return Ok(status);
        }

        [HttpPost("batch")]
        public IActionResult SetBatchStatus(
            [FromServices] CrabImportContext context,
            [FromBody] BatchStatusUpdate batchStatus)
        {
            context.SetCurrent(batchStatus);
            context.SaveChanges();

            return Ok();
        }

        [HttpGet("status/{feed}")]
        public IActionResult GetStatus(
            [FromServices] CrabImportContext context,
            [FromRoute] string feed)
            => Ok(context.StatusFor((ImportFeed)feed));

        [HttpGet("status")]
        public IActionResult GetStatus(
            [FromServices] CrabImportContext context)
            => Ok(context.StatusForAllFeeds());
    }

    public class RegisterCrabImportResponseExamples : IExamplesProvider<object>
    {
        public object GetExamples() => new { };
    }
}
