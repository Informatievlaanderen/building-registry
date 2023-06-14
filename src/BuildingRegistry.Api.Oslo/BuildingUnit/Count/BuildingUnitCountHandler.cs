namespace BuildingRegistry.Api.Oslo.BuildingUnit.Count
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
    using Projections.Syndication;
    using Query;

    public class BuildingUnitCountHandler : IRequestHandler<CountRequest, TotaalAantalResponse>
    {
        private readonly LegacyContext _context;
        private readonly SyndicationContext _syndicationContext;

        public BuildingUnitCountHandler(
            LegacyContext context,
            SyndicationContext syndicationContext)
        {
            _context = context;
            _syndicationContext = syndicationContext;
        }

        public async Task<TotaalAantalResponse> Handle(CountRequest request, CancellationToken cancellationToken)
        {
            var pagination = new NoPaginationRequest();

            return new TotaalAantalResponse
            {
                Aantal = request.FilteringHeader.ShouldFilter
                    ? await new BuildingUnitListOsloQuery(_context, _syndicationContext)
                        .Fetch(request.FilteringHeader, request.SortingHeader, pagination)
                        .Items
                        .CountAsync(cancellationToken)
                    : Convert.ToInt32(_context
                        .BuildingUnitDetailListCountView
                        .First()
                        .Count)
            };
        }
    }
}
