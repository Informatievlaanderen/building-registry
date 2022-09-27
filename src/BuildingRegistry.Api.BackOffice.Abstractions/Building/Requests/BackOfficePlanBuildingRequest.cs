namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests
{
    using Newtonsoft.Json;
    using System.Runtime.Serialization;

    [DataContract(Name = "PlanGebouw", Namespace = "")]
    public class BackOfficePlanBuildingRequest
    {
        /// <summary>
        /// De schets van het gebouw in GML-3 formaat met Lambert 72 referentie systeem.
        /// </summary>
        [DataMember(Name = "GeometriePolygoon", Order = 0)]
        [JsonProperty(Required = Required.Always)]
        public string GeometriePolygoon { get; set; }
    }
}
