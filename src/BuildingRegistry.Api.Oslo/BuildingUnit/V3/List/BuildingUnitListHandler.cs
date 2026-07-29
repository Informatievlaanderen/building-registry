namespace BuildingRegistry.Api.Oslo.BuildingUnit.V3.List
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
    using Query;

    public class BuildingUnitListHandler : IRequestHandler<ListRequest, BuildingUnitListOsloV3Response>
    {
        private readonly LegacyContext _context;
        private readonly IOptions<ResponseOptionsV3> _responseOptions;

        public BuildingUnitListHandler(
            LegacyContext context,
            IOptions<ResponseOptionsV3> responseOptions)
        {
            _context = context;
            _responseOptions = responseOptions;
        }

        public async Task<BuildingUnitListOsloV3Response> Handle(ListRequest request, CancellationToken cancellationToken)
        {
            var pagedBuildingUnits = new BuildingUnitListOsloQuery(_context)
                .Fetch(request.FilteringHeader, request.SortingHeader, request.PaginationRequest);

            var units = await pagedBuildingUnits.Items.ToListAsync(cancellationToken);

            return new BuildingUnitListOsloV3Response
            {
                Gebouweenheden = units
                    .Select(x => new GebouweenheidCollectieItemOsloV3(
                        x.BuildingUnitPersistentLocalId,
                        _responseOptions.Value.GebouweenheidDetailUrl,
                        x.Status.MapOslo(),
                        x.Version.ToBelgianDateTimeOffset()))
                    .ToList(),
                Volgende = pagedBuildingUnits
                    .PaginationInfo
                    .BuildNextUri(units.Count, _responseOptions.Value.GebouweenheidVolgendeUrl)!,
                Context = _responseOptions.Value.ContextUrlUnitList,
                Sorting = pagedBuildingUnits.Sorting,
                Pagination = pagedBuildingUnits.PaginationInfo
            };
        }
    }
}
