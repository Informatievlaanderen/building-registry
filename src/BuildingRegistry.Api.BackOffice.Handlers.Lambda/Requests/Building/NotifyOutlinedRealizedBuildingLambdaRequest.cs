namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.Building
{
    using BuildingRegistry.Building;
    using MediatR;

    public class NotifyOutlinedRealizedBuildingLambdaRequest : IRequest
    {
        public int BuildingPersistentLocalId { get; }
        public string Organisation { get; }
        public DateTimeOffset DateTimeStatusChange { get; }
        public ExtendedWkbGeometry ExtendedWkbGeometry { get; }

        public NotifyOutlinedRealizedBuildingLambdaRequest(
            int buildingPersistentLocalId,
            string organisation,
            DateTimeOffset dateTimeStatusChange,
            ExtendedWkbGeometry extendedWkbGeometry)
        {
            BuildingPersistentLocalId = buildingPersistentLocalId;
            Organisation = organisation;
            DateTimeStatusChange = dateTimeStatusChange;
            ExtendedWkbGeometry = extendedWkbGeometry;
        }
    }
}
