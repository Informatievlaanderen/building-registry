namespace BuildingRegistry.Api.Legacy.Building.Count
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Projections.Legacy;
    using Query;

    public class BuildingCountHandler : IRequestHandler<CountRequest, TotaalAantalResponse>
    {
        private readonly LegacyContext _context;

        public BuildingCountHandler(
            LegacyContext context)
        {
            _context = context;
        }

        public async Task<TotaalAantalResponse> Handle(CountRequest request, CancellationToken cancellationToken)
        {
            return new TotaalAantalResponse
            {
                Aantal = request.BuildingFilterHeader.ShouldFilter
                    ? await new BuildingListQuery(_context)
                        .Fetch(request.BuildingFilterHeader, request.SortingHeader, new NoPaginationRequest())
                        .Items
                        .CountAsync(cancellationToken)
                    : Convert.ToInt32(_context
                        .BuildingDetailListCountView
                        .First()
                        .Count)
            };

        }
    }
}
