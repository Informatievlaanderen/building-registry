namespace BuildingRegistry.Api.Legacy.Handlers.Building
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Building.Responses;
    using Api.Legacy.Abstractions.Building.Query;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Projections.Legacy;

    public record CrabGebouwenRequest(LegacyContext Context, HttpRequest HttpRequest) : IRequest<BuildingCrabMappingResponse?>;

    public class CrabGebouwenHandler : IRequestHandler<CrabGebouwenRequest, BuildingCrabMappingResponse?>
    {
        public async Task<BuildingCrabMappingResponse?> Handle(CrabGebouwenRequest request, CancellationToken cancellationToken)
        {
            var filtering = request.HttpRequest.ExtractFilteringRequest<BuildingCrabMappingFilter>();
            var sorting = request.HttpRequest.ExtractSortingRequest();
            var pagination = new NoPaginationRequest();

            if (filtering.Filter.TerrainObjectId == null && string.IsNullOrEmpty(filtering.Filter.IdentifierTerrainObject))
            {
                return null;
            }

            var query = new BuildingCrabMappingQuery(request.Context).Fetch(filtering, sorting, pagination);
            return new BuildingCrabMappingResponse
            {
                CrabGebouwen = query
                    .Items
                    .Select(x => new BuildingCrabMappingItem(x.PersistentLocalId.Value, x.CrabTerrainObjectId.Value, x.CrabIdentifierTerrainObject))
                    .ToList()
            };
        }
    }
}
