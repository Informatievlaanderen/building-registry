namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Requests;
    using System.Collections.Generic;

    public sealed class PlanBuildingSqsRequest : SqsRequest
    {
        public int BuildingPersistentLocalId { get; set; }


        public PlanBuildingRequest Request { get; set; }
    }

    public sealed class PlanBuildingSqsRequestFactory
    {
        private readonly IPersistentLocalIdGenerator _idGenerator;

        public PlanBuildingSqsRequestFactory(IPersistentLocalIdGenerator idGenerator)
        {
            _idGenerator = idGenerator;
        }

        public PlanBuildingSqsRequest Create(PlanBuildingRequest request, IDictionary<string, object?> metaData, ProvenanceData provenanceData)
        {
            return new PlanBuildingSqsRequest
            {
                BuildingPersistentLocalId = _idGenerator.GenerateNextPersistentLocalId(),
                Request = request,
                Metadata = metaData,
                ProvenanceData = provenanceData,
            };
        }
    }
}
