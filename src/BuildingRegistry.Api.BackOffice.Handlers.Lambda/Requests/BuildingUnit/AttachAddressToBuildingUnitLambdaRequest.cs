namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.BuildingUnit
{
    using Abstractions;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using Sqs.Requests.BuildingUnit;

    public sealed record AttachAddressToBuildingUnitLambdaRequest :
        BuildingUnitLambdaRequest,
        IHasBackOfficeRequest<AttachAddressToBuildingUnitBackOfficeRequest>
    {
        public AttachAddressToBuildingUnitBackOfficeRequest Request { get; }

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
