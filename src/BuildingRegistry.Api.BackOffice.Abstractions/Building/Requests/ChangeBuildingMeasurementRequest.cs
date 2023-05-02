namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests
{
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;

    [DataContract(Name = "WijzigGeometrieIngemetenGebouw", Namespace = "")]
    public sealed class ChangeBuildingMeasurementRequest
    {
        [DataMember(Name = "GrbData", Order = 0)]
        [JsonProperty(Required = Required.Always)]
        public GrbData GrbData { get; set; }

        public ChangeBuildingMeasurement ToCommand(int persistentLocalId, Provenance provenance)
            => new ChangeBuildingMeasurement(new BuildingPersistentLocalId(persistentLocalId),
                GrbData.GeometriePolygoon.ToExtendedWkbGeometry(),
                GrbData.ToBuildingGrbData(),
                provenance);
    }

    public class ChangeBuildingMeasurementRequestExamples : IExamplesProvider<ChangeBuildingMeasurementRequest>
    {
        public ChangeBuildingMeasurementRequest GetExamples()
        {
            return new ChangeBuildingMeasurementRequest
            {
                GrbData = new GrbData
                {
                    GeometriePolygoon  =  "<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:exterior><gml:LinearRing><gml:posList>140284.15277253836 186724.74131567031 140291.06016454101 186726.38355567306 140288.22675654292 186738.25798767805 140281.19098053873 186736.57913967967 140284.15277253836 186724.74131567031</gml:posList></gml:LinearRing></gml:exterior></gml:Polygon>"
                }
            };
        }
    }
}
