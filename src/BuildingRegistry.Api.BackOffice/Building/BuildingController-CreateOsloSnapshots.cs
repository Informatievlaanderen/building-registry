namespace BuildingRegistry.Api.BackOffice.Building
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.SqsRequests;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    public partial class BuildingController
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
            [FromBody] CreateBuildingOsloSnapshotsRequest request,
            CancellationToken cancellationToken = default)
        {
            var provenance = ProvenanceFactory.Create(new Reason(request.Reden), Modification.Unknown);

            var sqsRequest = new CreateBuildingOsloSnapshotsSqsRequest
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
