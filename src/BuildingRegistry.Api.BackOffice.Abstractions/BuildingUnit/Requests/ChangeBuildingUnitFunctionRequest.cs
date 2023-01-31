namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests
{
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using Converters;
    using MediatR;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;
    using System.Collections.Generic;

    public class ChangeBuildingUnitFunctionRequest : ChangeBuildingUnitFunctionBackOfficeRequest, IRequest<ETagResponse>
    {
        [JsonIgnore]
        public int BuildingUnitPersistentLocalId { get; set; }

        [JsonIgnore]
        public IDictionary<string, object> Metadata { get; set; }

        public ChangeBuildingUnitFunction ToCommand(
            BuildingPersistentLocalId buildingPersistentLocalId,
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            Provenance provenance)
        {
            return new ChangeBuildingUnitFunction(
                buildingPersistentLocalId,
                buildingUnitPersistentLocalId,
                Functie.Map(),
                provenance);
        }
    }

    public class ChangeBuildingUnitFunctionRequestExamples : IExamplesProvider<ChangeBuildingUnitFunctionRequest>
    {
        public ChangeBuildingUnitFunctionRequest GetExamples()
        {
            return new ChangeBuildingUnitFunctionRequest
            {
                Functie = GebouweenheidFunctie.Wonen
            };
        }
    }
}
