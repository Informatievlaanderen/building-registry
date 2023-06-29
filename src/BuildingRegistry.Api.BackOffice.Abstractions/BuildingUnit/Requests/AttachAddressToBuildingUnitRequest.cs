namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests
{
    using System.Runtime.Serialization;
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
