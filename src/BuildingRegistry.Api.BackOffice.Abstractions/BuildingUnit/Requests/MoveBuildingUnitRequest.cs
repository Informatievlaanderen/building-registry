namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests
{
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;
    using System.Runtime.Serialization;

    [DataContract(Name = "VerplaatsGebouweenheid", Namespace = "")]
    public sealed class MoveBuildingUnitRequest
    {
        /// <summary>
        /// De unieke en persistente identificator van het doelgebouw.
        /// </summary>
        [DataMember(Name = "DoelgebouwId", Order = 0)]
        [JsonProperty(Required = Required.Always)]
        public string DoelgebouwId { get; set; }
    }

    public class MoveBuildingUnitRequestExamples : IExamplesProvider<MoveBuildingUnitRequest>
    {
        public MoveBuildingUnitRequest GetExamples()
        {
            return new MoveBuildingUnitRequest
            {
                DoelgebouwId = "https://data.vlaanderen.be/id/gebouw/6447380"
            };
        }
    }
}
