namespace BuildingRegistry.Api.Oslo.BuildingUnit.V3.ChangeFeed
{
    using Infrastructure.Options;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json.Linq;
    using Swashbuckle.AspNetCore.Filters;

    public sealed class BuildingUnitFeedResultExample : IExamplesProvider<object>
    {
        private readonly ResponseOptionsV3 _responseOptions;

        public BuildingUnitFeedResultExample(IOptions<ResponseOptionsV3> responseOptions)
        {
            _responseOptions = responseOptions.Value;
        }

        public object GetExamples()
        {
            var json = $$"""
                         [
                            {
                                "specversion": "1.0",
                                 "id": "1",
                                 "time": "2023-11-02T07:24:43.9121352+01:00",
                                 "type": "basisregisters.buildingunit.create.v1",
                                 "source": "{{_responseOptions.BuildingUnitFeed?.FeedUrl}}",
                                 "datacontenttype": "application/json",
                                 "dataschema": "{{_responseOptions.BuildingUnitFeed?.DataSchemaUrl}}",
                                 "basisregisterseventtype": "BuildingWasMigrated",
                                 "basisregisterscausationid": "1af00df5-93ff-5319-a073-ca4bcc6b28f0",
                                 "subject": "https://data.vlaanderen.be/id/gebouweenheid/6356866",
                                 "data": {
                                     "objectId": "6356866",
                                     "naamruimte": "https://data.vlaanderen.be/id/gebouweenheid",
                                     "versieId": "2023-11-02T07:24:43+01:00",
                                     "nisCodes": [
                                         "46013"
                                     ],
                                     "attributen": [
                                         {
                                             "naam": "status",
                                             "oudeWaarde": null,
                                             "nieuweWaarde": "https://data.vlaanderen.be/id/concept/gebouweenheidstatus/gerealiseerd"
                                         },
                                         {
                                             "naam": "functie",
                                             "oudeWaarde": null,
                                             "nieuweWaarde": "https://data.vlaanderen.be/id/concept/gebouweenheidfunctie/nietGekend"
                                         },
                                         {
                                             "naam": "positie.methode",
                                             "oudeWaarde": null,
                                             "nieuweWaarde": "https://data.vlaanderen.be/id/concept/geometriemethode/aangeduidDoorBeheerder"
                                         },
                                         {
                                             "naam": "positie.geometrie",
                                             "oudeWaarde": null,
                                             "nieuweWaarde": [
                                                 {
                                                     "type": "Punt",
                                                     "gml": "<gml:Point srsName=\"http://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>144401.12 201731.91</gml:pos></gml:Point>"
                                                 },
                                                 {
                                                     "type": "Punt",
                                                     "gml": "<gml:Point srsName=\"http://www.opengis.net/def/crs/EPSG/0/3812\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>644397.11 701731.49</gml:pos></gml:Point>"
                                                 }
                                             ]
                                         },
                                         {
                                             "naam": "toegekendAdres",
                                             "oudeWaarde": null,
                                             "nieuweWaarde": [
                                                 "https://data.vlaanderen.be/id/adres/2434522"
                                             ]
                                         },
                                         {
                                             "naam": "isDeelVan",
                                             "oudeWaarde": null,
                                             "nieuweWaarde": "https://data.vlaanderen.be/id/gebouw/6355606"
                                         },
                                         {
                                             "naam": "afwijkingVastgesteld",
                                             "oudeWaarde": null,
                                             "nieuweWaarde": false
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
