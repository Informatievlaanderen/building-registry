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
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using Building;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Exceptions;
    using FluentValidation;
    using FluentValidation.Results;
    using Handlers.Sqs.Requests.BuildingUnit;
    using Infrastructure;
    using Infrastructure.Options;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using Swashbuckle.AspNetCore.Filters;

    public partial class BuildingUnitController
    {
        /// <summary>
        /// Realiseer een gebouweenheid niet.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="ifMatchHeaderValidator"></param>
        /// <param name="request"></param>
        /// <param name="ifMatchHeaderValue"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPost("{buildingUnitPersistentLocalId}/acties/nietrealiseren")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseHeader(StatusCodes.Status202Accepted, "location", "string", "De url van de niet gerealiseerde gebouweenheid.")]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> NotRealize(
            [FromServices] IOptions<ResponseOptions> options,
            [FromServices] IIfMatchHeaderValidator ifMatchHeaderValidator,
            [FromRoute] NotRealizeBuildingUnitRequest request,
            [FromHeader(Name = "If-Match")] string? ifMatchHeaderValue,
            CancellationToken ct = default)
        {
            try
            {
                if (!await ifMatchHeaderValidator
                        .IsValidForBuildingUnit(ifMatchHeaderValue, new BuildingUnitPersistentLocalId(request.BuildingUnitPersistentLocalId), ct))
                {
                    return new PreconditionFailedResult();
                }

                if (UseSqsToggle.FeatureEnabled)
                {
                    var result = await Mediator.Send(
                        new NotRealizeBuildingUnitSqsRequest
                        {
                            Request = request,
                            Metadata = GetMetadata(),
                            ProvenanceData = new ProvenanceData(CreateFakeProvenance()),
                            IfMatchHeaderValue = ifMatchHeaderValue
                        }, ct);

                    return Accepted(result);
                }

                request.Metadata = GetMetadata();
                var response = await Mediator.Send(request, ct);

                return new AcceptedWithETagResult(
                    new Uri(string.Format(options.Value.BuildingUnitDetailUrl, request.BuildingUnitPersistentLocalId)),
                    response.ETag);
            }
            catch (AggregateIdIsNotFoundException)
            {
                throw new ApiException(ValidationErrorMessages.BuildingUnit.BuildingUnitNotFound, StatusCodes.Status404NotFound);
            }
            catch (IdempotencyException)
            {
                return Accepted();
            }
            catch (AggregateNotFoundException)
            {
                throw CreateValidationException(
                    ValidationErrorCodes.BuildingUnit.BuildingNotFound,
                    string.Empty,
                    ValidationErrorMessages.BuildingUnit.BuildingNotFound);
            }
            catch (DomainException exception)
            {
                throw exception switch
                {
                    BuildingUnitIsNotFoundException => new ApiException(ValidationErrorMessages.BuildingUnit.BuildingUnitNotFound, StatusCodes.Status404NotFound),

                    BuildingUnitIsRemovedException => new ApiException(ValidationErrorMessages.BuildingUnit.BuildingUnitIsRemoved, StatusCodes.Status410Gone),

                    BuildingUnitHasInvalidStatusException =>
                        CreateValidationException(
                            ValidationErrorCodes.BuildingUnit.BuildingUnitCannotBeNotRealized,
                            string.Empty,
                            ValidationErrorMessages.BuildingUnit.BuildingUnitCannotBeNotRealized),

                    _ => new ValidationException(new List<ValidationFailure>
                        { new ValidationFailure(string.Empty, exception.Message) })
                };
            }
        }
    }
}
