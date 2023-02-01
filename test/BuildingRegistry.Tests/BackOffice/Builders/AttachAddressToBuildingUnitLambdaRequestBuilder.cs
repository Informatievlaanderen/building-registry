namespace BuildingRegistry.Tests.BackOffice.Builders
{
    using System;
    using System.Collections.Generic;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.SqsRequests;
    using BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.BuildingUnit;

    public class AttachAddressToBuildingUnitLambdaRequestBuilder
    {
        private readonly Fixture _fixture;

        private BuildingPersistentLocalId? _buildingPersistentLocalId;
        private BuildingUnitPersistentLocalId? _buildingUnitPersistentLocalId;
        private string? _adresId;
        private Guid? _ticketId;
        private string? _ifMatchHeaderValue;

        public AttachAddressToBuildingUnitLambdaRequestBuilder(Fixture fixture)
        {
            _fixture = fixture;
        }

        public AttachAddressToBuildingUnitLambdaRequestBuilder WithBuildingPersistentLocalId(BuildingPersistentLocalId buildingPersistentLocalId)
        {
            _buildingPersistentLocalId = buildingPersistentLocalId;
            return this;
        }

        public AttachAddressToBuildingUnitLambdaRequestBuilder WithBuildingUnitPersistentLocalId(BuildingUnitPersistentLocalId buildingUnitPersistentLocalId)
        {
            _buildingUnitPersistentLocalId = buildingUnitPersistentLocalId;
            return this;
        }

        public AttachAddressToBuildingUnitLambdaRequestBuilder WithAdresId(int addressPersistentLocalId)
        {
            _adresId = PuriCreator.CreateAdresId(addressPersistentLocalId);
            return this;
        }

        public AttachAddressToBuildingUnitLambdaRequestBuilder WithTicketId(
            Guid ticketId)
        {
            _ticketId = ticketId;
            return this;
        }

        public AttachAddressToBuildingUnitLambdaRequestBuilder WithIfMatchHeaderValue(
            string ifMatchHeaderValue)
        {
            _ifMatchHeaderValue = ifMatchHeaderValue;
            return this;
        }

        public AttachAddressToBuildingUnitLambdaRequest Build()
        {
            var buildingPersistentLocalId = _buildingPersistentLocalId ?? _fixture.Create<BuildingPersistentLocalId>();
            var buildingUnitPersistentLocalId = _buildingUnitPersistentLocalId ?? _fixture.Create<BuildingUnitPersistentLocalId>();
            var ticketId = _ticketId ?? _fixture.Create<Guid>();
            var adresId = _adresId ?? PuriCreator.CreateAdresId(123);

            return new AttachAddressToBuildingUnitLambdaRequest(
                messageGroupId: buildingPersistentLocalId,
                new AttachAddressToBuildingUnitSqsRequest()
                {
                    BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId,
                    Request = new AttachAddressToBuildingUnitRequest { AdresId = adresId },
                    TicketId = ticketId,
                    IfMatchHeaderValue = _ifMatchHeaderValue,
                    Metadata = new Dictionary<string, object?>(),
                    ProvenanceData = _fixture.Create<ProvenanceData>()
                }
            );
        }
    }
}
