namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests
{
    using System.Collections.Generic;

    public class CreateBuildingUnitOsloSnapshotsRequest
    {
        public List<int> BuildingUnitPersistentLocalIds { get; set; }

        public string Reden { get; set; }
    }
}
