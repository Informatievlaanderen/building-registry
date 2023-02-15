namespace BuildingRegistry.Api.Legacy.Building.Crab
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using BuildingRegistry.Api.Legacy.Building.Query;
    using MediatR;

    public class CrabGebouwenHandler : IRequestHandler<CrabGebouwenRequest, BuildingCrabMappingResponse?>
    {
        public Task<BuildingCrabMappingResponse?> Handle(CrabGebouwenRequest request, CancellationToken cancellationToken)
        {
            if (request.FilteringHeader.Filter.TerrainObjectId == null && string.IsNullOrEmpty(request.FilteringHeader.Filter.IdentifierTerrainObject))
            {
                return Task.FromResult((BuildingCrabMappingResponse?)null);
            }

            var query = new BuildingCrabMappingQuery(request.Context).Fetch(request.FilteringHeader, request.SortingHeader, new NoPaginationRequest());
            return Task.FromResult<BuildingCrabMappingResponse?>(new BuildingCrabMappingResponse
            {
                CrabGebouwen = query
                    .Items
                    .Select(x => new BuildingCrabMappingItem(x.PersistentLocalId.Value, x.CrabTerrainObjectId.Value, x.CrabIdentifierTerrainObject))
                    .ToList()
            });
        }
    }
}
