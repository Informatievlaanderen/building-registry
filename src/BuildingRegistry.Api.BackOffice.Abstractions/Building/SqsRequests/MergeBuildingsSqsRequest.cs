namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Requests;
    using System.Collections.Generic;

    public sealed class MergeBuildingsSqsRequest : SqsRequest
    {
        public int BuildingPersistentLocalId { get; set; }
        public MergeBuildingRequest Request { get; set; }
    }

    public sealed class MergeBuildingsSqsRequestFactory
    {
        private readonly IPersistentLocalIdGenerator _idGenerator;

        public MergeBuildingsSqsRequestFactory(IPersistentLocalIdGenerator idGenerator)
        {
            _idGenerator = idGenerator;
        }

        public MergeBuildingsSqsRequest Create(
            MergeBuildingRequest request,
            IDictionary<string, object?> metaData,
            ProvenanceData provenanceData)
        {
            return new MergeBuildingsSqsRequest
            {
                BuildingPersistentLocalId = _idGenerator.GenerateNextPersistentLocalId(),
                Request = request,
                Metadata = metaData,
                ProvenanceData = provenanceData,
            };
        }
    }
}
