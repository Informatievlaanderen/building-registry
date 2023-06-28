namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;

    [DataContract(Name = "SamenvoegenGebouw", Namespace = "")]
    public sealed class MergeBuildingRequest
    {
        /// <summary>
        /// De schets van het samengevoegd gebouw in GML-3 formaat met Lambert 72 referentie systeem.
        /// </summary>
        [DataMember(Name = "GeometriePolygoon", Order = 0)]
        [JsonProperty(Required = Required.Always)]
        public string GeometriePolygoon { get; set; }

        /// <summary>
        /// Lijst met gebouwen dat wordt samengevoegd tot het doelgebouw.
        /// </summary>
        [DataMember(Name = "SamenvoegenGebouwen", Order = 1)]
        [JsonProperty(Required = Required.Always)]
        public List<string> SamenvoegenGebouwen { get; set; }
    }

    public class MergeBuildingRequestExamples : IExamplesProvider<MergeBuildingRequest>
    {
        public MergeBuildingRequest GetExamples()
        {
            return new MergeBuildingRequest
            {
                GeometriePolygoon = "<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:exterior><gml:LinearRing><gml:posList>140284.15277253836 186724.74131567031 140291.06016454101 186726.38355567306 140288.22675654292 186738.25798767805 140281.19098053873 186736.57913967967 140284.15277253836 186724.74131567031</gml:posList></gml:LinearRing></gml:exterior></gml:Polygon>",
                SamenvoegenGebouwen = new List<string>
                {
                    "https://data.vlaanderen.be/id/gebouw/200001",
                    "https://data.vlaanderen.be/id/gebouw/200002",
                }
            };
        }
    }
}
