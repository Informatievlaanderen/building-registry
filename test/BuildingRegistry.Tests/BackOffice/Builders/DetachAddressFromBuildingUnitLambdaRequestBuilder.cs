namespace BuildingRegistry.Tests.BackOffice.Builders
{
    using System;
    using System.Collections.Generic;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.BuildingUnit;
    using BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests.BuildingUnit;

    public class DetachAddressFromBuildingUnitLambdaRequestBuilder
    {
        private readonly Fixture _fixture;

        private BuildingPersistentLocalId? _buildingPersistentLocalId;
        private BuildingUnitPersistentLocalId? _buildingUnitPersistentLocalId;
        private string? _adresId;
        private Guid? _ticketId;
        private string? _ifMatchHeaderValue;

        public DetachAddressFromBuildingUnitLambdaRequestBuilder(Fixture fixture)
        {
            _fixture = fixture;
        }

        public DetachAddressFromBuildingUnitLambdaRequestBuilder WithBuildingPersistentLocalId(BuildingPersistentLocalId buildingPersistentLocalId)
        {
            _buildingPersistentLocalId = buildingPersistentLocalId;
            return this;
        }

        public DetachAddressFromBuildingUnitLambdaRequestBuilder WithBuildingUnitPersistentLocalId(BuildingUnitPersistentLocalId buildingUnitPersistentLocalId)
        {
            _buildingUnitPersistentLocalId = buildingUnitPersistentLocalId;
            return this;
        }

        public DetachAddressFromBuildingUnitLambdaRequestBuilder WithAdresId(int addressPersistentLocalId)
        {
            _adresId = PuriCreator.CreateAdresId(addressPersistentLocalId);
            return this;
        }

        public DetachAddressFromBuildingUnitLambdaRequestBuilder WithTicketId(
            Guid ticketId)
        {
            _ticketId = ticketId;
            return this;
        }

        public DetachAddressFromBuildingUnitLambdaRequestBuilder WithIfMatchHeaderValue(
            string ifMatchHeaderValue)
        {
            _ifMatchHeaderValue = ifMatchHeaderValue;
            return this;
        }

        public DetachAddressFromBuildingUnitLambdaRequest Build()
        {
            var buildingPersistentLocalId = _buildingPersistentLocalId ?? _fixture.Create<BuildingPersistentLocalId>();
            var buildingUnitPersistentLocalId = _buildingUnitPersistentLocalId ?? _fixture.Create<BuildingUnitPersistentLocalId>();
            var ticketId = _ticketId ?? _fixture.Create<Guid>();
            var adresId = _adresId ?? PuriCreator.CreateAdresId(123);

            return new DetachAddressFromBuildingUnitLambdaRequest(
                messageGroupId: buildingPersistentLocalId,
                new DetachAddressFromBuildingUnitSqsRequest()
                {
                    BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId,
                    Request = new DetachAddressFromBuildingUnitRequest { AdresId = adresId },
                    TicketId = ticketId,
                    IfMatchHeaderValue = _ifMatchHeaderValue,
                    Metadata = new Dictionary<string, object?>(),
                    ProvenanceData = _fixture.Create<ProvenanceData>()
                }
            );
        }
    }
}
