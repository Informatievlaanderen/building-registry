namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using MediatR;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;

    public class ChangeBuildingOutlineRequest : ChangeBuildingOutlineBackOfficeRequest, IRequest<ETagResponse>
    {
        /// <summary>
        /// De unieke en persistente identificator van het gebouw.
        /// </summary>
        [JsonIgnore]
        public int PersistentLocalId { get; set; }

        [JsonIgnore]
        public IDictionary<string, object> Metadata { get; set; }

        public ChangeBuildingOutline ToCommand(
            BuildingPersistentLocalId buildingPersistentLocalId,
            Provenance provenance)
        {
            return new ChangeBuildingOutline(
                buildingPersistentLocalId,
                GeometriePolygoon.ToExtendedWkbGeometry(),
                provenance);
        }
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
