namespace BuildingRegistry.Api.Oslo.BuildingUnit.Count
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using BuildingRegistry.Api.Oslo.BuildingUnit.Query;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Projections.Legacy;

    public class BuildingUnitCountHandlerV2 : IRequestHandler<CountRequest, TotaalAantalResponse>
    {
        private readonly LegacyContext _context;

        public BuildingUnitCountHandlerV2(
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
                    ? await new BuildingUnitListOsloQueryV2(_context)
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
