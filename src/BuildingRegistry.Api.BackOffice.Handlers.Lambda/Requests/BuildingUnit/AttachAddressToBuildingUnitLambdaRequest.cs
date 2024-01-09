namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.BuildingUnit
{
    using Abstractions.Building.Validators;
    using Abstractions.BuildingUnit.Requests;
    using Abstractions.BuildingUnit.SqsRequests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;

    public sealed record AttachAddressToBuildingUnitLambdaRequest : BuildingUnitLambdaRequest
    {
        public AttachAddressToBuildingUnitRequest Request { get; }

        public int BuildingUnitPersistentLocalId { get; }

        public AttachAddressToBuildingUnitLambdaRequest(
            string messageGroupId,
            AttachAddressToBuildingUnitSqsRequest sqsRequest)
            : base(messageGroupId,
                sqsRequest.TicketId,
                sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata)
        {
            BuildingUnitPersistentLocalId = sqsRequest.BuildingUnitPersistentLocalId;
            Request = sqsRequest.Request;
        }

        /// <summary>
        /// Map to command
        /// </summary>
        /// <returns>AttachAddressToBuildingUnit.</returns>
        public AttachAddressToBuildingUnit ToCommand()
        {
            var addressPersistentLocalId = OsloPuriValidatorExtensions.ParsePersistentLocalId(Request.AdresId);

            return new AttachAddressToBuildingUnit(
                BuildingPersistentLocalId,
                new BuildingUnitPersistentLocalId(BuildingUnitPersistentLocalId),
                new AddressPersistentLocalId(addressPersistentLocalId),
                Provenance);
        }
    }
}
