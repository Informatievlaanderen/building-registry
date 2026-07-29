namespace BuildingRegistry.Api.Oslo.BuildingUnit.V3.Count
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Projections.Legacy;
    using Query;

    public class BuildingUnitCountHandler : IRequestHandler<CountRequest, TotaalAantalResponse>
    {
        private readonly LegacyContext _context;

        public BuildingUnitCountHandler(
            LegacyContext context)
        {
            _context = context;
        }

        public async Task<TotaalAantalResponse> Handle(CountRequest request, CancellationToken cancellationToken)
        {
            var pagination = new NoPaginationRequest();

            return new TotaalAantalResponse
            {
                Aantal = request.FilteringHeader.ShouldFilter
                    ? await new BuildingUnitListOsloQuery(_context)
                        .Fetch(request.FilteringHeader, request.SortingHeader, pagination)
                        .Items
                        .CountAsync(cancellationToken)
                    : Convert.ToInt32(_context
                        .BuildingUnitDetailListCountViewV2
                        .First()
                        .Count)
            };
        }
    }
}
