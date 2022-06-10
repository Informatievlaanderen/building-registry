namespace BuildingRegistry.Api.BackOffice.Building
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Building.Requests;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using FluentValidation;
    using Handlers.Building;
    using Infrastructure.Options;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using Microsoft.Net.Http.Headers;
    using Swashbuckle.AspNetCore.Filters;

    public partial class BuildingController
    {
        /// <summary>
        /// Plan een gebouw met schets in.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="planBuildingRequest"></param>
        /// <param name="validator"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="202">Als het gebouw (reeds) ingepland is.</response>
        /// <returns></returns>
        [HttpPost("acties/plannen")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseHeader(StatusCodes.Status202Accepted, "location", "string", "De url van het geplande gebouw.")]
        [SwaggerRequestExample(typeof(PlanBuildingRequest), typeof(PlanBuildingRequestExamples))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]

        public async Task<IActionResult> Plan(
            [FromServices] IOptions<ResponseOptions> options,
            [FromServices] IValidator<PlanBuildingRequest> validator,
            [FromBody] PlanBuildingRequest planBuildingRequest,
            CancellationToken cancellationToken = default)
        {
            await validator.ValidateAndThrowAsync(planBuildingRequest, cancellationToken);

            try
            {
                planBuildingRequest.Metadata = GetMetadata();
                var response = await _mediator.Send(planBuildingRequest, cancellationToken);

                return new AcceptedWithETagResult(
                    new Uri(string.Format(options.Value.BuildingDetailUrl, response.BuildingPersistentLocalId)),
                    response.LastEventHash);
            }
            catch (IdempotencyException)
            {
                return Accepted();
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }


    //TODO: move to lib after queue system is approved
    public class AcceptedWithETagResult : AcceptedResult
    {
        private readonly List<string> _tags;
        public string ETag { get; }

        public AcceptedWithETagResult(Uri location, string eTag, params string[] tags) : base(location, null)
        {
            _tags = new List<string>(tags);
            ETag = eTag;
        }

        public override async Task ExecuteResultAsync(ActionContext context)
        {
            AddLastObservedPositionAsEtag(context);
            await base.ExecuteResultAsync(context);
        }

        public override void ExecuteResult(ActionContext context)
        {
            AddLastObservedPositionAsEtag(context);
            base.ExecuteResult(context);
        }

        private void AddLastObservedPositionAsEtag(ActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            _tags.Add(ETag);
            var etag = new ETag(ETagType.Strong, string.Join("-", _tags));

            if (context.HttpContext.Response.Headers.Keys.Contains(HeaderNames.ETag))
            {
                context.HttpContext.Response.Headers[HeaderNames.ETag] = etag.ToString();
            }
            else
            {
                context.HttpContext.Response.Headers.Add(HeaderNames.ETag, etag.ToString());
            }
        }
    }
}
