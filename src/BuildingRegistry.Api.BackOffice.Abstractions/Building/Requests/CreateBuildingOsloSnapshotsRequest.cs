namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests
{
    using System.Collections.Generic;

    public class CreateBuildingOsloSnapshotsRequest
    {
        public List<int> BuildingPersistentLocalIds { get; set; }

        public string Reden { get; set; }
    }
}
