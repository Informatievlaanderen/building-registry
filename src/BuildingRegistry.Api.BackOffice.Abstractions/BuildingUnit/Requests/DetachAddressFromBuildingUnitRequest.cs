namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;

    [DataContract(Name = "OntkoppelAdres", Namespace = "")]
    public sealed class DetachAddressFromBuildingUnitRequest
    {
        /// <summary>
        /// De unieke en persistente identificator van het adres.
        /// </summary>
        [DataMember(Name = "AdresId", Order = 0)]
        [JsonProperty(Required = Required.Always)]
        public string AdresId { get; set; }

        [JsonIgnore]
        public int BuildingUnitPersistentLocalId { get; set; }

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
