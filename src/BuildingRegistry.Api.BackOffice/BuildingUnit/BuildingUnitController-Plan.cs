namespace BuildingRegistry.Api.BackOffice.BuildingUnit
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Building.Requests;
    using Abstractions.Building.Validators;
    using Abstractions.BuildingUnit.Requests;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Validators;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using Building;
    using BuildingRegistry.Building.Exceptions;
    using FluentValidation;
    using FluentValidation.Results;
    using Handlers.Sqs.Requests.BuildingUnit;
    using Infrastructure.Options;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using Swashbuckle.AspNetCore.Filters;

    public partial class BuildingUnitController
    {
        /// <summary>
        /// Plan een gebouweenheid in.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="buildingExistsValidator"></param>
        /// <param name="request"></param>
        /// <param name="validator"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("acties/plannen")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseHeader(StatusCodes.Status202Accepted, "location", "string", "De url van de geplande gebouweenheid.")]
        [SwaggerRequestExample(typeof(PlanBuildingRequest), typeof(PlanBuildingRequestExamples))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> Plan(
            [FromServices] IOptions<ResponseOptions> options,
            [FromServices] IValidator<PlanBuildingUnitRequest> validator,
            [FromBody] PlanBuildingUnitRequest request,
            CancellationToken cancellationToken = default)
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken);

            try
            {
                if (UseSqsToggle.FeatureEnabled)
                {
                    OsloPuriValidator.TryParseIdentifier(request.GebouwId, out var buildingIdentifier);

                    var result = await Mediator.Send(
                        new PlanBuildingUnitSqsRequest
                        {
                            Request = request,
                            Metadata = GetMetadata(),
                            ProvenanceData = new ProvenanceData(CreateFakeProvenance()),
                        }, cancellationToken);

                    return Accepted(result);
                }

                request.Metadata = GetMetadata();
                var response = await Mediator.Send(request, cancellationToken);

                return new AcceptedWithETagResult(
                    new Uri(string.Format(options.Value.BuildingUnitDetailUrl, response.BuildingUnitPersistentLocalId)),
                    response.LastEventHash);
            }
            catch (IdempotencyException)
            {
                return Accepted();
            }
            catch (AggregateNotFoundException)
            {
                throw CreateValidationException(
                    ValidationErrorCodes.BuildingUnit.BuildingNotFound,
                    nameof(request.GebouwId),
                    ValidationErrorMessages.BuildingUnit.BuildingInvalid(request.GebouwId));
            }
            catch (DomainException exception)
            {
                throw exception switch
                {
                    BuildingHasInvalidStatusException =>
                        throw CreateValidationException(
                            ValidationErrorCodes.BuildingUnit.BuildingUnitCannotBePlanned,
                            nameof(request.GebouwId),
                            ValidationErrorMessages.BuildingUnit.BuildingUnitCannotBePlanned),

                    BuildingUnitPositionIsOutsideBuildingGeometryException =>
                        throw CreateValidationException(
                            ValidationErrorCodes.BuildingUnit.BuildingUnitOutsideGeometryBuilding,
                            string.Empty,
                            ValidationErrorMessages.BuildingUnit.BuildingUnitOutsideGeometryBuilding),

                    _ => new ValidationException(new List<ValidationFailure>
                        { new ValidationFailure(string.Empty, exception.Message) })
                };
            }
        }
    }
}
