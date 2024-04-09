namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests
{
    using Newtonsoft.Json;
    using System.Runtime.Serialization;

    public sealed class MoveBuildingUnitRequest
    {
        /// <summary>
        /// De unieke en persistente identificator van het doelgebouw.
        /// </summary>
        [DataMember(Name = "DoelgebouwId", Order = 0)]
        [JsonProperty(Required = Required.Always)]
        public string DoelgebouwId { get; set; }

        //[JsonIgnore]
        //public int BuildingUnitPersistentLocalId { get; set; }
    }
}
