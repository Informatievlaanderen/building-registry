namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using Converters;
    using MediatR;
    using Newtonsoft.Json;
    using Responses;
    using Swashbuckle.AspNetCore.Filters;

    public class PlanBuildingUnitRequest : PlanBuildingUnitBackOfficeRequest, IRequest<PlanBuildingUnitResponse>
    {
        [JsonIgnore]
        public IDictionary<string, object> Metadata { get; set; }

        public PlanBuildingUnit ToCommand(
            BuildingPersistentLocalId buildingPersistentLocalId,
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            Provenance provenance)
            => new PlanBuildingUnit(
                buildingPersistentLocalId,
                buildingUnitPersistentLocalId,
                PositieGeometrieMethode.Map(),
                string.IsNullOrWhiteSpace(Positie) ? null : Positie.ToExtendedWkbGeometry(),
                Functie.Map(),
                AfwijkingVastgesteld,
                provenance);
    }

    public class PlanBuildingUnitRequestExamples : IExamplesProvider<PlanBuildingUnitRequest>
    {
        public PlanBuildingUnitRequest GetExamples()
        {
            return new PlanBuildingUnitRequest
            {
                GebouwId = "https://data.vlaanderen.be/id/gebouw/6447380",
                PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                Positie = "<gml:Point srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>103671.37 192046.71</gml:pos></gml:Point>",
                Functie = GebouweenheidFunctie.NietGekend,
                AfwijkingVastgesteld = false
            };
        }
    }
}
