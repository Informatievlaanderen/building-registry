namespace BuildingRegistry.Api.Oslo.BuildingUnit.ChangeFeed
{
    using Infrastructure.Options;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json.Linq;
    using Swashbuckle.AspNetCore.Filters;

    public sealed class BuildingUnitFeedResultExample : IExamplesProvider<object>
    {
        private readonly ResponseOptions _responseOptions;

        public BuildingUnitFeedResultExample(IOptions<ResponseOptions> responseOptions)
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
                                 "time": "2023-11-02T07:24:43.9174449+01:00",
                                 "type": "basisregisters.buildingunit.create.v1",
                                 "source": "{{_responseOptions.BuildingUnitFeed?.FeedUrl}}",
                                 "datacontenttype": "application/json",
                                 "dataschema": "{{_responseOptions.BuildingUnitFeed?.DataSchemaUrl}}",
                                 "basisregisterseventtype": "BuildingUnitWasPlannedV2",
                                 "basisregisterscausationid": "e675177c-b243-550d-acff-1a4891bbf669",
                                 "data": {
                                     "@id": "https://data.vlaanderen.be/id/gebouweenheid/6763967",
                                     "objectId": "6763967",
                                     "naamruimte": "https://data.vlaanderen.be/id/gebouweenheid",
                                     "versieId": "2023-11-02T07:24:43+01:00",
                                     "nisCodes": [
                                         "23052"
                                     ],
                                     "attributen": [
                                         {
                                             "naam": "gebouweenheidStatus",
                                             "oudeWaarde": null,
                                             "nieuweWaarde": "gepland"
                                         },
                                         {
                                             "naam": "gebouweenheidFunctie",
                                             "oudeWaarde": null,
                                             "nieuweWaarde": "nietGekend"
                                         },
                                         {
                                             "naam": "positieGeometrieMethode",
                                             "oudeWaarde": null,
                                             "nieuweWaarde": "aangeduidDoorBeheerder"
                                         },
                                         {
                                             "naam": "gebouweenheidPositie",
                                             "oudeWaarde": null,
                                             "nieuweWaarde": [
                                                 {
                                                     "type": "Point",
                                                     "projectie": "http://www.opengis.net/def/crs/EPSG/0/31370",
                                                     "gml": "<gml:Point srsName=\"http://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>140284.15277253836 186724.74131567031</gml:pos></gml:Point>"
                                                 },
                                                 {
                                                     "type": "Point",
                                                     "projectie": "http://www.opengis.net/def/crs/EPSG/0/3812",
                                                     "gml": "<gml:Point srsName=\"http://www.opengis.net/def/crs/EPSG/0/3812\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>640279.35 686715.64</gml:pos></gml:Point>"
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
