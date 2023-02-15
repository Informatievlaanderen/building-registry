namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Requests;
    using Microsoft.AspNetCore.Components.Forms;
    using System.Collections.Generic;
    using System.Threading;

    public sealed class PlanBuildingUnitSqsRequest : SqsRequest
    {
        public int BuildingUnitPersistentLocalId { get; set; }

        public PlanBuildingUnitRequest Request { get; set; }
    }

    public sealed class PlanBuildingUnitSqsRequestFactory
    {
        private readonly IPersistentLocalIdGenerator _idGenerator;

        public PlanBuildingUnitSqsRequestFactory(IPersistentLocalIdGenerator idGenerator)
        {
            _idGenerator = idGenerator;
        }

        public PlanBuildingUnitSqsRequest Create(PlanBuildingUnitRequest request, IDictionary<string, object?> metaData, ProvenanceData provenanceData)
        {
            return new PlanBuildingUnitSqsRequest
            {
                BuildingUnitPersistentLocalId = _idGenerator.GenerateNextPersistentLocalId(),
                Request = request,
                Metadata = metaData,
                ProvenanceData = provenanceData,
            };
        }
    }
}
