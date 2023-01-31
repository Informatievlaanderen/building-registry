using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests
{
    [DataContract(Name = "WijzigGebouweenheidFunctie", Namespace = "")]
    public class ChangeBuildingUnitFunctionBackOfficeRequest
    {
        /// <summary>
        /// De functie van de gebouweenheid.
        /// </summary>
        [DataMember(Name = "Functie", Order = 1)]
        [JsonProperty(Required = Required.Always)]
        public GebouweenheidFunctie Functie { get; set; }
    }
}
