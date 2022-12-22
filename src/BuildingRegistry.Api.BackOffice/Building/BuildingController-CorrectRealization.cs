namespace BuildingRegistry.Api.BackOffice.Building
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Building.Requests;
    using Abstractions.Building.Validators;
    using Abstractions.Validation;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Exceptions;
    using FluentValidation;
    using FluentValidation.Results;
    using Handlers.Sqs.Requests.Building;
    using Infrastructure;
    using Infrastructure.Options;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using Swashbuckle.AspNetCore.Filters;

    public partial class BuildingController
    {
        /// <summary>
        /// Corrigeer realisatie gebouw.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="buildingExistsValidator"></param>
        /// <param name="ifMatchHeaderValidator"></param>
        /// <param name="request"></param>
        /// <param name="validator"></param>
        /// <param name="ifMatchHeaderValue"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="202">Aanvraag tot correctie realisatie wordt reeds verwerkt.</response>
        /// <response code="412">Als de If-Match header niet overeenkomt met de laatste ETag.</response>
        /// <returns></returns>
        [HttpPost("{persistentLocalId}/acties/corrigeren/realisering")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status412PreconditionFailed)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> CorrectRealization(
            [FromServices] IOptions<ResponseOptions> options,
            [FromServices] IValidator<CorrectBuildingRealizationRequest> validator,
            [FromServices] BuildingExistsValidator buildingExistsValidator,
            [FromServices] IIfMatchHeaderValidator ifMatchHeaderValidator,
            [FromRoute] CorrectBuildingRealizationRequest request,
            [FromHeader(Name = "If-Match")] string? ifMatchHeaderValue,
            CancellationToken cancellationToken = default)
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken);

            try
            {
                request.Metadata = GetMetadata();

                if (!await ifMatchHeaderValidator.IsValidForBuilding(
                        ifMatchHeaderValue,
                        new BuildingPersistentLocalId(request.PersistentLocalId),
                        cancellationToken))
                {
                    return new PreconditionFailedResult();
                }

                if (UseSqsToggle.FeatureEnabled)
                {
                    if (!await buildingExistsValidator.Exists(new BuildingPersistentLocalId(request.PersistentLocalId), cancellationToken))
                    {
                        throw new ApiException(ValidationErrors.Common.BuildingNotFound.Message, StatusCodes.Status404NotFound);
                    }

                    var result = await Mediator.Send(
                        new CorrectBuildingRealizationSqsRequest
                        {
                            Request = request,
                            Metadata = GetMetadata(),
                            ProvenanceData = new ProvenanceData(CreateFakeProvenance()),
                            IfMatchHeaderValue = ifMatchHeaderValue
                        }, cancellationToken);

                    return Accepted(result);
                }

                var response = await Mediator.Send(request, cancellationToken);

                return new AcceptedWithETagResult(
                    new Uri(string.Format(options.Value.BuildingDetailUrl, request.PersistentLocalId)),
                    response.ETag);
            }
            catch (IdempotencyException)
            {
                return Accepted();
            }
            catch (AggregateNotFoundException)
            {
                throw new ApiException(ValidationErrors.Common.BuildingNotFound.Message, StatusCodes.Status404NotFound);
            }
            catch (DomainException exception)
            {
                throw exception switch
                {
                    BuildingIsRemovedException => new ApiException(ValidationErrors.Common.BuildingIsRemoved.Message, StatusCodes.Status410Gone),
                    BuildingHasInvalidStatusException => CreateValidationException(
                        ValidationErrorCodes.Building.BuildingCannotBeCorrectedFromRealizedToUnderConstruction,
                        string.Empty,
                        ValidationErrorMessages.Building.BuildingCannotBeCorrectedFromRealizedToUnderConstruction),
                    BuildingHasInvalidBuildingGeometryMethodException =>CreateValidationException(
                        ValidationErrorCodes.Building.BuildingIsMeasuredByGrb,
                        string.Empty,
                        ValidationErrorMessages.Building.BuildingIsMeasuredByGrb),
                    BuildingHasRetiredBuildingUnitsException => CreateValidationException(
                        ValidationErrorCodes.Building.BuildingHasRetiredBuildingUnits,
                        string.Empty,
                        ValidationErrorMessages.Building.BuildingHasRetiredBuildingUnits),

                    _ => new ValidationException(new List<ValidationFailure>
                        { new ValidationFailure(string.Empty, exception.Message) })
                };
            }
        }
    }
}
