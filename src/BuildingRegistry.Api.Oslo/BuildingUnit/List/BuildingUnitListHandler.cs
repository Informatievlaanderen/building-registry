namespace BuildingRegistry.Api.Oslo.BuildingUnit.List
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Converters;
    using Infrastructure;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Projections.Legacy;
    using Projections.Syndication;
    using Query;

    public class ListHandler : IRequestHandler<ListRequest, BuildingUnitListOsloResponse>
    {
        private readonly LegacyContext _context;
        private readonly SyndicationContext _syndicationContext;
        private readonly IOptions<ResponseOptions> _responseOptions;

        public ListHandler(
            LegacyContext context,
            SyndicationContext syndicationContext,
            IOptions<ResponseOptions> responseOptions)
        {
            _context = context;
            _syndicationContext = syndicationContext;
            _responseOptions = responseOptions;
        }

        public async Task<BuildingUnitListOsloResponse> Handle(ListRequest request, CancellationToken cancellationToken)
        {
            var pagedBuildingUnits = new BuildingUnitListOsloQuery(_context, _syndicationContext)
                .Fetch(request.FilteringHeader, request.SortingHeader, request.PaginationRequest);

            var units = await pagedBuildingUnits.Items
                .Select(a => new
                {
                    a.PersistentLocalId,
                    a.Version,
                    a.Status
                })
                .ToListAsync(cancellationToken);

            return new BuildingUnitListOsloResponse
            {
                Gebouweenheden = units
                    .Select(x => new GebouweenheidCollectieItemOslo(
                        x.PersistentLocalId.Value,
                        _responseOptions.Value.GebouweenheidNaamruimte,
                        _responseOptions.Value.GebouweenheidDetailUrl,
                        x.Status.Value.Map(),
                        x.Version.ToBelgianDateTimeOffset()))
                    .ToList(),
                Volgende = pagedBuildingUnits
                    .PaginationInfo
                    .BuildNextUri(units.Count, _responseOptions.Value.GebouweenheidVolgendeUrl),
                Context = _responseOptions.Value.ContextUrlUnitList,
                Sorting = pagedBuildingUnits.Sorting,
                Pagination = pagedBuildingUnits.PaginationInfo
            };
        }
    }
}
