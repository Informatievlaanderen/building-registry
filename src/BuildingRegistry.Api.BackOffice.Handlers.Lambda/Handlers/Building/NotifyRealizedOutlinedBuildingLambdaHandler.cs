namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Handlers.Building
{
    using GrbAnoApi;
    using MediatR;
    using OrWegwijsApi;
    using Requests.Building;

    public class NotifyRealizedOutlinedBuildingLambdaHandler : IRequestHandler<NotifyOutlinedRealizedBuildingLambdaRequest>
    {
        public const string GeoIt = "Geo-IT";

        private readonly IWegwijsApiProxy _wegwijsApiProxy;
        private readonly IAnoApiProxy _apiProxy;

        public NotifyRealizedOutlinedBuildingLambdaHandler(
            IWegwijsApiProxy wegwijsApiProxy,
            IAnoApiProxy apiProxy)
        {
            _wegwijsApiProxy = wegwijsApiProxy;
            _apiProxy = apiProxy;
        }

        public async Task Handle(
            NotifyOutlinedRealizedBuildingLambdaRequest request,
            CancellationToken cancellationToken)
        {
            var organisation = request.Organisation;
            if (IsOvoCode(organisation))
            {
                organisation = await _wegwijsApiProxy.GetOrganisationName(organisation);
            }
            else if (IsGeoIt(organisation))
            {
                organisation = GeoIt;
            }

            await _apiProxy.CreateAnomaly(
                request.BuildingPersistentLocalId,
                request.DateTimeStatusChange,
                organisation,
                request.ExtendedWkbGeometry,
                cancellationToken);
        }

        private bool IsGeoIt(string organisation)
        {
            var sanitized = organisation
                .Replace(".", string.Empty)
                .Replace(" ", string.Empty);

            // KBO number Geo-IT 0867.526.230
            return sanitized.Equals("0867526230");
        }

        private static bool IsOvoCode(string organisation) => organisation.StartsWith("OVO", StringComparison.InvariantCultureIgnoreCase);
    }
}
