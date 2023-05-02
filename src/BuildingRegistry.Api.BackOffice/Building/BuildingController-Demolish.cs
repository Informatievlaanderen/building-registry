namespace BuildingRegistry.Api.BackOffice.Building
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Building.Requests;
    using Abstractions.Building.SqsRequests;
    using Abstractions.Building.Validators;
    using Abstractions.Validation;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Building;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using NodaTime;
    using Swashbuckle.AspNetCore.Filters;

    public partial class BuildingController
    {
        /// <summary>
        /// Gebouw slopen.
        /// </summary>
        /// <param name="buildingExistsValidator"></param>
        /// <param name="request"></param>
        /// <param name="persistentLocalId"></param>
        /// <param name="cancellationToken"></param>
        [HttpPost("{persistentLocalId}/acties/slopen")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseHeader(StatusCodes.Status202Accepted, "location", "string", "De url van het gebouw.")]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> Demolish(
            [FromServices] BuildingExistsValidator buildingExistsValidator,
            [FromBody] DemolishBuildingRequest request,
            [FromRoute] int persistentLocalId,
            CancellationToken cancellationToken = default)
        {
            if (!await buildingExistsValidator.Exists(new BuildingPersistentLocalId(persistentLocalId), cancellationToken))
            {
                throw new ApiException(ValidationErrors.Common.BuildingNotFound.Message, StatusCodes.Status404NotFound);
            }

            var result = await Mediator.Send(
                new DemolishBuildingSqsRequest
                {
                    BuildingPersistentLocalId = new BuildingPersistentLocalId(persistentLocalId),
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
