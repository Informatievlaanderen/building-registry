namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests
{
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;

    [DataContract(Name = "WijzigGeometrieGeschetstGebouw", Namespace = "")]
    public sealed class ChangeBuildingOutlineRequest
    {
        /// <summary>
        /// De schets van het gebouw in GML-3 formaat met Lambert 72 referentie systeem.
        /// </summary>
        [DataMember(Name = "GeometriePolygoon", Order = 0)]
        [JsonProperty(Required = Required.Always)]
        public string GeometriePolygoon { get; set; }

        /// <summary>
        /// De unieke en persistente identificator van het gebouw.
        /// </summary>
        [JsonIgnore]
        public int PersistentLocalId { get; set; }
    }

    public class ChangeBuildingOutlineRequestExamples : IExamplesProvider<ChangeBuildingOutlineRequest>
    {
        public ChangeBuildingOutlineRequest GetExamples()
        {
            return new ChangeBuildingOutlineRequest
            {
                GeometriePolygoon = "<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:exterior><gml:LinearRing><gml:posList>140284.15277253836 186724.74131567031 140291.06016454101 186726.38355567306 140288.22675654292 186738.25798767805 140281.19098053873 186736.57913967967 140284.15277253836 186724.74131567031</gml:posList></gml:LinearRing></gml:exterior></gml:Polygon>"
            };
        }
    }
}
