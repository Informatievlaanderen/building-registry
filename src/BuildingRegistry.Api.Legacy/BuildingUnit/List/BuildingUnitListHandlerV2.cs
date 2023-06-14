namespace BuildingRegistry.Api.Legacy.BuildingUnit.List
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Infrastructure;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Projections.Legacy;
    using Query;

    public class BuildingUnitListHandlerV2 : IRequestHandler<BuildingUnitListRequest, BuildingUnitListResponse>
    {
        private readonly LegacyContext _context;
        private readonly IOptions<ResponseOptions> _responseOptions;

        public BuildingUnitListHandlerV2(
            LegacyContext context,
            IOptions<ResponseOptions> responseOptions)
        {
            _context = context;
            _responseOptions = responseOptions;
        }

        public async Task<BuildingUnitListResponse> Handle(BuildingUnitListRequest request, CancellationToken cancellationToken)
        {
            var pagedBuildingUnits = new BuildingUnitListQueryV2(_context)
                .Fetch(request.FilteringHeader, request.SortingHeader, request.PaginationRequest);

            var units = await pagedBuildingUnits.Items
                .Select(a => new
                {
                    a.BuildingUnitPersistentLocalId,
                    a.Version,
                    a.Status
                })
                .ToListAsync(cancellationToken);

            return new BuildingUnitListResponse
            {
                Gebouweenheden = units
                    .Select(x => new GebouweenheidCollectieItem(
                        x.BuildingUnitPersistentLocalId,
                        _responseOptions.Value.GebouweenheidNaamruimte,
                        _responseOptions.Value.GebouweenheidDetailUrl,
                        x.Status.Map(),
                        x.Version.ToBelgianDateTimeOffset()))
                    .ToList(),
                Volgende = pagedBuildingUnits
                    .PaginationInfo
                    .BuildNextUri(units.Count, _responseOptions.Value.GebouweenheidVolgendeUrl),
                Sorting = pagedBuildingUnits.Sorting,
                Pagination = pagedBuildingUnits.PaginationInfo
            };
        }
    }
}
