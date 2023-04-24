namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.SqsRequests
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Requests;

    public sealed class RealizeAndMeasureUnplannedBuildingSqsRequest : SqsRequest
    {
        public int BuildingPersistentLocalId { get; set; }
        public RealizeAndMeasureUnplannedBuildingRequest Request { get; set; }
    }

    public sealed class RealizeAndMeasureUnplannedBuildingSqsRequestFactory
    {
        private readonly IPersistentLocalIdGenerator _idGenerator;

        public RealizeAndMeasureUnplannedBuildingSqsRequestFactory(IPersistentLocalIdGenerator idGenerator)
        {
            _idGenerator = idGenerator;
        }

        public RealizeAndMeasureUnplannedBuildingSqsRequest Create(RealizeAndMeasureUnplannedBuildingRequest request, IDictionary<string, object?> metaData, ProvenanceData provenanceData)
        {
            return new RealizeAndMeasureUnplannedBuildingSqsRequest
            {
                BuildingPersistentLocalId = _idGenerator.GenerateNextPersistentLocalId(),
                Request = request,
                Metadata = metaData,
                ProvenanceData = provenanceData,
            };
        }
    }
}
