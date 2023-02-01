namespace BuildingRegistry.Api.Legacy.Handlers.Building
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Building;
    using Abstractions.Building.Responses;
    using Api.Legacy.Abstractions.Building.Query;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using MediatR;

    public class CrabGebouwenHandler : IRequestHandler<CrabGebouwenRequest, BuildingCrabMappingResponse?>
    {
        public Task<BuildingCrabMappingResponse?> Handle(CrabGebouwenRequest request, CancellationToken cancellationToken)
        {
            var filtering = request.HttpRequest.ExtractFilteringRequest<BuildingCrabMappingFilter>();
            var sorting = request.HttpRequest.ExtractSortingRequest();
            var pagination = new NoPaginationRequest();

            if (filtering.Filter.TerrainObjectId == null && string.IsNullOrEmpty(filtering.Filter.IdentifierTerrainObject))
            {
                return Task.FromResult((BuildingCrabMappingResponse?)null);
            }

            var query = new BuildingCrabMappingQuery(request.Context).Fetch(filtering, sorting, pagination);
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
