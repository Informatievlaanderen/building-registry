namespace BuildingRegistry.Api.BackOffice.BuildingUnit
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.SqsRequests;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    public partial class BuildingUnitController
    {
        /// <summary>
        /// Creëer nieuwe OSLO snapshots.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("acties/oslosnapshots")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = PolicyNames.GeschetstGebouw.InterneBijwerker)]
        public async Task<IActionResult> CreateOsloSnapshots(
            [FromBody] CreateOsloSnapshotsRequest request,
            CancellationToken cancellationToken = default)
        {
            var provenance = ProvenanceFactory.Create(new Reason(request.Reden), Modification.Unknown);

            var sqsRequest = new CreateOsloSnapshotsSqsRequest
            {
                Request = request,
                Metadata = GetMetadata(),
                ProvenanceData = new ProvenanceData(provenance)
            };

            var sqsResult = await Mediator.Send(sqsRequest, cancellationToken);

            return Accepted(sqsResult);
        }
    }
}
