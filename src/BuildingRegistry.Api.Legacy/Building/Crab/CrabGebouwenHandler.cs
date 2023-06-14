namespace BuildingRegistry.Api.Legacy.Building.Crab
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using MediatR;
    using Projections.Legacy;
    using Query;

    public class CrabGebouwenHandler : IRequestHandler<CrabGebouwenRequest, BuildingCrabMappingResponse?>
    {
        private readonly LegacyContext _context;

        public CrabGebouwenHandler(
            LegacyContext context)
        {
            _context = context;
        }

        public Task<BuildingCrabMappingResponse?> Handle(CrabGebouwenRequest request, CancellationToken cancellationToken)
        {
            if (request.FilteringHeader.Filter.TerrainObjectId == null && string.IsNullOrEmpty(request.FilteringHeader.Filter.IdentifierTerrainObject))
            {
                return Task.FromResult((BuildingCrabMappingResponse?)null);
            }

            var query = new BuildingCrabMappingQuery(_context).Fetch(request.FilteringHeader, request.SortingHeader, new NoPaginationRequest());
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
