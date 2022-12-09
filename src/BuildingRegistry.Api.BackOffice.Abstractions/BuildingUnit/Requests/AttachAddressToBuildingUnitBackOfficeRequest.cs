using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests
{
    [DataContract(Name = "KoppelAdres", Namespace = "")]
    public class AttachAddressToBuildingUnitBackOfficeRequest
    {
        /// <summary>
        /// Adres welke dient gekoppeld te worden aan de gebouweenheid.
        /// </summary>
        [DataMember(Name = "AdresId", Order = 0)]
        [JsonProperty(Required = Required.Always)]
        public string AdresId { get; set; }
    }
}
