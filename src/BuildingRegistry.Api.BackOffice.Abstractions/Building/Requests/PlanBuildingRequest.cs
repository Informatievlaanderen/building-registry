namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using MediatR;
    using Newtonsoft.Json;
    using Responses;
    using Swashbuckle.AspNetCore.Filters;
    
    public class PlanBuildingRequest : BackOfficePlanBuildingRequest, IRequest<PlanBuildingResponse>
    {
        [JsonIgnore]
        public IDictionary<string, object> Metadata { get; set; }

        public PlanBuilding ToCommand(
            BuildingPersistentLocalId buildingPersistentLocalId,
            Provenance provenance)
        {
            return new PlanBuilding(buildingPersistentLocalId,
                GeometriePolygoon.ToExtendedWkbGeometry(),
                provenance);
        }
    }

    public class PlanBuildingRequestExamples : IExamplesProvider<PlanBuildingRequest>
    {
        public PlanBuildingRequest GetExamples()
        {
            return new PlanBuildingRequest
            {
                GeometriePolygoon = "<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:exterior><gml:LinearRing><gml:posList>140284.15277253836 186724.74131567031 140291.06016454101 186726.38355567306 140288.22675654292 186738.25798767805 140281.19098053873 186736.57913967967 140284.15277253836 186724.74131567031</gml:posList></gml:LinearRing></gml:exterior></gml:Polygon>"
            };
        }
    }
}
