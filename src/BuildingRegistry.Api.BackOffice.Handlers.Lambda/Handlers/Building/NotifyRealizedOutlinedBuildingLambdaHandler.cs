namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Handlers.Building
{
    using MediatR;
    using Requests.Building;

    public class NotifyRealizedOutlinedBuildingLambdaHandler : IRequestHandler<NotifyOutlinedRealizedBuildingLambdaRequest>
    {
        private readonly IAnoApiProxy _apiProxy;

        public NotifyRealizedOutlinedBuildingLambdaHandler(IAnoApiProxy apiProxy)
        {
            _apiProxy = apiProxy;
        }

        public async Task Handle(
            NotifyOutlinedRealizedBuildingLambdaRequest request,
            CancellationToken cancellationToken)
        {
            await _apiProxy.CreateAnomaly(
                request.BuildingPersistentLocalId,
                request.DateTimeStatusChange,
                request.Organisation,
                request.ExtendedWkbGeometry,
                cancellationToken);
        }
    }
}
