namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using MediatR;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;

    public class DetachAddressFromBuildingUnitRequest : DetachAddressFromBuildingUnitBackOfficeRequest, IRequest<ETagResponse>
    {
        [JsonIgnore]
        public int BuildingUnitPersistentLocalId { get; set; }

        [JsonIgnore]
        public IDictionary<string, object> Metadata { get; set; }

        public DetachAddressFromBuildingUnit ToCommand(
            BuildingPersistentLocalId buildingPersistentLocalId,
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            Provenance provenance)
        {
            var addressPersistentLocalId = OsloPuriValidatorExtensions.ParsePersistentLocalId(AdresId);

            return new DetachAddressFromBuildingUnit(
                buildingPersistentLocalId,
                buildingUnitPersistentLocalId,
                new AddressPersistentLocalId(addressPersistentLocalId),
                provenance);
        }
    }

    public class DetachAddressFromBuildingUnitRequestExamples : IExamplesProvider<DetachAddressFromBuildingUnitRequest>
    {
        public DetachAddressFromBuildingUnitRequest GetExamples()
        {
            return new DetachAddressFromBuildingUnitRequest
            {
                AdresId = "https://data.vlaanderen.be/id/adres/6447380"
            };
        }
    }
}
