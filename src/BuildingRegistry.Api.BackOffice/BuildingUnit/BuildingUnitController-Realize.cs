namespace BuildingRegistry.Api.BackOffice.BuildingUnit
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Building.Validators;
    using Abstractions.BuildingUnit.Requests;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Building;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Extensions;
    using BuildingRegistry.Building.Exceptions;
    using FluentValidation;
    using FluentValidation.Results;
    using Handlers;
    using Infrastructure.Options;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using Swashbuckle.AspNetCore.Filters;

    public partial class BuildingUnitController
    {
        /// <summary>
        /// Realiseer een gebouweenheid..
        /// </summary>
        /// <param name="options"></param>
        /// <param name="request"></param>
        /// <param name="ifMatchHeaderValue"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("{buildingUnitPersistentLocalId}/acties/realiseren")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseHeader(StatusCodes.Status202Accepted, "location", "string", "De url van de gerealiseerde gebouweenheid.")]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> Realize(
            [FromServices] IOptions<ResponseOptions> options,
            [FromRoute] RealizeBuildingUnitRequest request,
            [FromHeader(Name = "If-Match")] string? ifMatchHeaderValue,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(ifMatchHeaderValue))
                {
                    var buildingPersistentLocalId = BackOfficeContext.GetBuildingIdForBuildingUnit(request.BuildingUnitPersistentLocalId);

                    var etag = await GetBuildingUnitEtag(buildingPersistentLocalId, request.BuildingUnitPersistentLocalId, cancellationToken);

                    if (!IfMatchHeaderMatchesEtag(ifMatchHeaderValue, etag))
                    {
                        return new PreconditionFailedResult();
                    }
                }

                request.Metadata = GetMetadata();
                var response = await _mediator.Send(request, cancellationToken);

                return new AcceptedWithETagResult(
                    new Uri(string.Format(options.Value.BuildingUnitDetailUrl, request.BuildingUnitPersistentLocalId)),
                    response.LastEventHash);
            }
            catch (IdempotencyException)
            {
                return Accepted();         
            }
            catch (AggregateNotFoundException)
            {
                throw CreateValidationException(
                    ValidationErrorCodes.Building.BuildingNotFound,
                    string.Empty,
                    ValidationErrorMessages.Building.BuildingNotFound);
            }
            catch (DomainException exception)
            {
                throw exception switch
                {
                    BuildingUnitNotFoundException => new ApiException(ValidationErrorMessages.BuildingUnit.BuildingUnitNotFound, StatusCodes.Status404NotFound),
                    
                    BuildingUnitIsRemovedException => new ApiException(ValidationErrorMessages.BuildingUnit.BuildingUnitIsRemoved, StatusCodes.Status410Gone),

                    BuildingUnitStatusPreventsBuildingUnitRealizationException => CreateValidationException(
                        ValidationErrorCodes.BuildingUnit.BuildingUnitCannotBeRealized,
                        string.Empty,
                        ValidationErrorMessages.BuildingUnit.BuildingUnitCannotBeRealized),

                    BuildingStatusPreventsBuildingUnitRealizationException => CreateValidationException(
                        ValidationErrorCodes.BuildingUnit.BuildingStatusNotInRealized,
                        string.Empty,
                        ValidationErrorMessages.BuildingUnit.BuildingStatusNotInRealized),

                    _ => new ValidationException(new List<ValidationFailure>
                        { new ValidationFailure(string.Empty, exception.Message) })
                };
            }
        }
    }
}
