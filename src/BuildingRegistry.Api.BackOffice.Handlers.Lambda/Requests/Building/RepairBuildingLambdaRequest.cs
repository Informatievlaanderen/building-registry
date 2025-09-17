namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.Building
{
    using Abstractions.Building.SqsRequests;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using NodaTime;
    using IHasBuildingPersistentLocalId = Abstractions.IHasBuildingPersistentLocalId;

    public sealed record RepairBuildingLambdaRequest : BuildingLambdaRequest, IHasBuildingPersistentLocalId
    {
        public int BuildingPersistentLocalId { get; }

        public RepairBuildingLambdaRequest(string messageGroupId, RepairBuildingSqsRequest sqsRequest)
            : base(messageGroupId, sqsRequest.TicketId, sqsRequest.IfMatchHeaderValue,
                new Provenance(
                    SystemClock.Instance.GetCurrentInstant(),
                    Application.BuildingRegistry,
                    new Reason("Herstel gebouw"),
                    new Operator("OVO002949"),
                    Modification.Update,
                    Organisation.DigitaalVlaanderen),
                sqsRequest.Metadata)
        {
            BuildingPersistentLocalId = sqsRequest.BuildingPersistentLocalId;
        }

        /// <summary>
        /// Map to RepairBuilding command.
        /// </summary>
        /// <returns>RepairBuilding.</returns>
        public RepairBuilding ToCommand()
        {
            return new RepairBuilding(new BuildingPersistentLocalId(BuildingPersistentLocalId), Provenance);
        }
    }
}
