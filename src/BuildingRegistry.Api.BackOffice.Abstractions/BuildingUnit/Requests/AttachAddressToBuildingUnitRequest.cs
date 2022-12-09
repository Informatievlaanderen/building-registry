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

    public class AttachAddressToBuildingUnitRequest : AttachAddressToBuildingUnitBackOfficeRequest, IRequest<ETagResponse>
    {
        [JsonIgnore]
        public int BuildingUnitPersistentLocalId { get; set; }

        [JsonIgnore]
        public IDictionary<string, object> Metadata { get; set; }

        public AttachAddressToBuildingUnit ToCommand(
            BuildingPersistentLocalId buildingPersistentLocalId,
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            Provenance provenance)
        {
            var addressPersistentLocalId = OsloPuriValidatorExtensions.ParsePersistentLocalId(AdresId);

            return new AttachAddressToBuildingUnit(
                buildingPersistentLocalId,
                buildingUnitPersistentLocalId,
                new AddressPersistentLocalId(addressPersistentLocalId),
                provenance);
        }
    }

    public class AttachAddressToBuildingUnitRequestExamples : IExamplesProvider<AttachAddressToBuildingUnitRequest>
    {
        public AttachAddressToBuildingUnitRequest GetExamples()
        {
            return new AttachAddressToBuildingUnitRequest
            {
                AdresId = "https://data.vlaanderen.be/id/adressen/6447380"
            };
        }
    }
}
