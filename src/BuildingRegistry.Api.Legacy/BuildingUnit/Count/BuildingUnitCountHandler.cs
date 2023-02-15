namespace BuildingRegistry.Api.Legacy.BuildingUnit.Count
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using BuildingRegistry.Api.Legacy.BuildingUnit.Query;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class BuildingUnitCountHandler : IRequestHandler<CountRequest, TotaalAantalResponse>
    {
        public async Task<TotaalAantalResponse> Handle(CountRequest request, CancellationToken cancellationToken)
        {
            return new TotaalAantalResponse
            {
                Aantal = request.FilteringHeader.ShouldFilter
                    ? await new BuildingUnitListQuery(request.Context, request.SyndicationContext)
                        .Fetch(request.FilteringHeader, request.SortingHeader, new NoPaginationRequest())
                        .Items
                        .CountAsync(cancellationToken)
                    : Convert.ToInt32(request.Context
                        .BuildingUnitDetailListCountView
                        .First()
                        .Count)
            };
        }
    }
}
