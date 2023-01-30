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

    [DataContract(Name = "KoppelAdres", Namespace = "")]
    public sealed class AttachAddressToBuildingUnitRequest
    {
        /// <summary>
        /// De unieke en persistente identificator van het adres.
        /// </summary>
        [DataMember(Name = "AdresId", Order = 0)]
        [JsonProperty(Required = Required.Always)]
        public string AdresId { get; set; }

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
                AdresId = "https://data.vlaanderen.be/id/adres/6447380"
            };
        }
    }
}
