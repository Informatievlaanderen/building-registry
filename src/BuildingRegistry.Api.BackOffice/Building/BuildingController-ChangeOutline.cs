namespace BuildingRegistry.Api.BackOffice.Building
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Building.Requests;
    using Abstractions.Building.Validators;
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
        /// Wijzig geometrie van een geschetst gebouw.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="validator"></param>
        /// <param name="buildingExistsValidator"></param>
        /// <param name="ifMatchHeaderValidator"></param>
        /// <param name="persistentLocalId"></param>
        /// <param name="request"></param>
        /// <param name="ifMatchHeaderValue"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="202">Als het ticket succesvol is aangemaakt.</response>
        /// <response code="412">Als de If-Match header niet overeenkomt met de laatste ETag.</response>
        /// <returns></returns>
        [HttpPost("{persistentLocalId}/acties/wijzigen/schetsgeometriepolygoon")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status412PreconditionFailed)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseHeader(StatusCodes.Status202Accepted, "location", "string", "De URL van het aangemaakte ticket.")]
        [SwaggerRequestExample(typeof(ChangeBuildingOutlineRequest), typeof(ChangeBuildingOutlineRequestExamples))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> ChangeOutline(
            [FromServices] IOptions<ResponseOptions> options,
            [FromServices] IValidator<ChangeBuildingOutlineRequest> validator,
            [FromServices] BuildingExistsValidator buildingExistsValidator,
            [FromServices] IIfMatchHeaderValidator ifMatchHeaderValidator,
            [FromRoute] int persistentLocalId,
            [FromBody] ChangeBuildingOutlineRequest request,
            [FromHeader(Name = "If-Match")] string? ifMatchHeaderValue,
            CancellationToken cancellationToken = default)
        {
            request.PersistentLocalId = persistentLocalId;

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
                        throw new ApiException(ValidationErrorMessages.Building.BuildingNotFound, StatusCodes.Status404NotFound);
                    }

                    var result = await Mediator.Send(
                        new ChangeBuildingOutlineSqsRequest
                        {
                            BuildingPersistentLocalId = persistentLocalId,
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
                throw new ApiException(ValidationErrorMessages.Building.BuildingNotFound, StatusCodes.Status404NotFound);
            }
            catch (DomainException exception)
            {
                throw exception switch
                {
                    BuildingIsRemovedException => new ApiException(ValidationErrorMessages.Building.BuildingRemoved, StatusCodes.Status410Gone),

                    BuildingHasInvalidBuildingGeometryMethodException => CreateValidationException(
                        ValidationErrorCodes.Building.BuildingIsMeasuredByGrb,
                        string.Empty,
                        ValidationErrorMessages.Building.BuildingIsMeasuredByGrb),

                    BuildingHasBuildingUnitsOutsideBuildingGeometryException => CreateValidationException(
                        ValidationErrorCodes.Building.BuildingHasBuildingUnitsOutsideChangedGeometry,
                        string.Empty,
                        ValidationErrorMessages.Building.BuildingHasBuildingUnitsOutsideChangedGeometry),

                    _ => new ValidationException(new List<ValidationFailure>
                        { new ValidationFailure(string.Empty, exception.Message) })
                };
            }
        }
    }
}
