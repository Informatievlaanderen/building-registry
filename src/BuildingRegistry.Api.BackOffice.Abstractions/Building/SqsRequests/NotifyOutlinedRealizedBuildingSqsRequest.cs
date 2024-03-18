namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.SqsRequests
{
    using System;

    public sealed class NotifyOutlinedRealizedBuildingSqsRequest
    {
        public int BuildingPersistentLocalId { get; }
        public string Organisation { get; set; }
        public DateTimeOffset DateTimeStatusChange { get; set; }
        public string ExtendedWkbGeometry { get; set; }

        public NotifyOutlinedRealizedBuildingSqsRequest(
            int buildingPersistentLocalId,
            string organisation,
            DateTimeOffset dateTimeStatusChange,
            string extendedWkbGeometry)
        {
            BuildingPersistentLocalId = buildingPersistentLocalId;
            Organisation = organisation;
            DateTimeStatusChange = dateTimeStatusChange;
            ExtendedWkbGeometry = extendedWkbGeometry;
        }
    }
}
