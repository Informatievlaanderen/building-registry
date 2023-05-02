namespace BuildingRegistry.Api.BackOffice.Building
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Building.Requests;
    using Abstractions.Building.SqsRequests;
    using Abstractions.Building.Validators;
    using Abstractions.Validation;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Building;
    using FluentValidation;
    using Infrastructure;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using NodaTime;
    using Swashbuckle.AspNetCore.Filters;

    public partial class BuildingController
    {
        /// <summary>
        /// Wijzig geometrie van een ingemeten gebouw.
        /// </summary>
        /// <param name="validator"></param>
        /// <param name="buildingExistsValidator"></param>
        /// <param name="persistentLocalId"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("{persistentLocalId}/acties/wijzigen/ingemetengeometriepolygoon")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status412PreconditionFailed)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseHeader(StatusCodes.Status202Accepted, "location", "string", "De URL van het aangemaakte ticket.")]
        [SwaggerRequestExample(typeof(ChangeBuildingMeasurementRequest), typeof(ChangeBuildingMeasurementRequestExamples))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> ChangeMeasurement(
            [FromServices] IValidator<ChangeBuildingMeasurementRequest> validator,
            [FromServices] BuildingExistsValidator buildingExistsValidator,
            [FromRoute] int persistentLocalId,
            [FromBody] ChangeBuildingMeasurementRequest request,
            CancellationToken cancellationToken = default)
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken);

            if (!await buildingExistsValidator.Exists(new BuildingPersistentLocalId(persistentLocalId), cancellationToken))
            {
                throw new ApiException(ValidationErrors.Common.BuildingNotFound.Message, StatusCodes.Status404NotFound);
            }

            var result = await Mediator.Send(
                new ChangeBuildingMeasurementSqsRequest
                {
                    BuildingPersistentLocalId = persistentLocalId,
                    Request = request,
                    Metadata = GetMetadata(),
                    ProvenanceData = new ProvenanceData(new Provenance(
                        SystemClock.Instance.GetCurrentInstant(),
                        Application.Grb,
                        new Reason(""),
                        new Operator(""),
                        Modification.Update,
                        Organisation.DigitaalVlaanderen))
                }, cancellationToken);

            return Accepted(result);
        }
    }
}
