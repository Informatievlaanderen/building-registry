namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.BuildingUnit
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using BuildingRegistry.Building;
    using NodaTime;

    public abstract record BuildingUnitLambdaRequest : SqsLambdaRequest
    {
        public BuildingPersistentLocalId BuildingPersistentLocalId => new BuildingPersistentLocalId(Convert.ToInt32(MessageGroupId));

        protected BuildingUnitLambdaRequest(
            string messageGroupId,
            Guid ticketId,
            string? ifMatchHeaderValue,
            Provenance provenance,
            IDictionary<string, object?> metadata)
            : base(messageGroupId, ticketId, ifMatchHeaderValue, provenance, metadata)
        { }

        protected Provenance CommandProvenance => new Provenance(
            SystemClock.Instance.GetCurrentInstant(),
            Provenance.Application,
            Provenance.Reason,
            Provenance.Operator,
            Provenance.Modification,
            Provenance.Organisation);
    }
}
