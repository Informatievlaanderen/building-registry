namespace BuildingRegistry.Api.Oslo.Building.ChangeFeed
{
    using Infrastructure.Options;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json.Linq;
    using Swashbuckle.AspNetCore.Filters;

    public sealed class BuildingFeedResultExample : IExamplesProvider<object>
    {
        private readonly ResponseOptions _responseOptions;

        public BuildingFeedResultExample(IOptions<ResponseOptions> responseOptions)
        {
            _responseOptions = responseOptions.Value;
        }

        public object GetExamples()
        {
            var json = $$"""
                         [
                            {
                                "specversion": "1.0",
                                 "id": "2",
                                 "time": "2023-11-02T07:37:09.2309729+01:00",
                                 "type": "basisregisters.building.create.v1",
                                 "source": "{{_responseOptions.BuildingFeed.FeedUrl}}",
                                 "datacontenttype": "application/json",
                                 "dataschema": "{{_responseOptions.BuildingFeed.DataSchemaUrl}}",
                                 "basisregisterseventtype": "BuildingWasMigrated",
                                 "basisregisterscausationid": "0870f9b0-bba0-5444-9f76-4316e9f8cc0f",
                                 "data": {
                                     "@id": "https://data.vlaanderen.be/id/gebouw/200001",
                                     "objectId": "200001",
                                     "naamruimte": "https://data.vlaanderen.be/id/gebouw",
                                     "versieId": "2023-11-02T07:37:09+01:00",
                                     "nisCodes": [
                                         "34042"
                                     ],
                                     "attributen": [
                                         {
                                             "naam": "gebouwStatus",
                                             "oudeWaarde": null,
                                             "nieuweWaarde": "gerealiseerd"
                                         },
                                         {
                                             "naam": "gebouwMethode",
                                             "oudeWaarde": null,
                                             "nieuweWaarde": "ingemetenGRB"
                                         },
                                         {
                                             "naam": "gebouwGeometrie",
                                             "oudeWaarde": null,
                                             "nieuweWaarde": [
                                                 {
                                                     "type": "Polygon",
                                                     "projectie": "http://www.opengis.net/def/crs/EPSG/0/31370",
                                                     "gml": "<gml:Polygon srsName=\"http://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:exterior><gml:LinearRing><gml:posList>140284.15277253836 186724.74131567031 140291.06016454101 186726.38355567306 140288.22675654292 186738.25055567548 140281.19098aborl54014 186736.57090367282 140284.15277253836 186724.74131567031</gml:posList></gml:LinearRing></gml:exterior></gml:Polygon>"
                                                 },
                                                 {
                                                     "type": "Polygon",
                                                     "projectie": "http://www.opengis.net/def/crs/EPSG/0/3812",
                                                     "gml": "<gml:Polygon srsName=\"http://www.opengis.net/def/crs/EPSG/0/3812\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:exterior><gml:LinearRing><gml:posList>640279.35 686715.64 640286.26 686717.28 640283.43 686729.15 640276.39 686727.47 640279.35 686715.64</gml:posList></gml:LinearRing></gml:exterior></gml:Polygon>"
                                                 }
                                             ]
                                         }
                                     ]
                                 }
                             }
                         ]
                         """;
            return JArray.Parse(json);
        }
    }
}
