namespace BuildingRegistry.Api.Legacy.Building.Count
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using BuildingRegistry.Api.Legacy.Building.Query;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class BuildingCountHandler : IRequestHandler<CountRequest, TotaalAantalResponse>
    {
        public async Task<TotaalAantalResponse> Handle(CountRequest request, CancellationToken cancellationToken)
        {
            return new TotaalAantalResponse
            {
                Aantal = request.BuildingFilterHeader.ShouldFilter
                    ? await new BuildingListQuery(request.Context)
                        .Fetch(request.BuildingFilterHeader, request.SortingHeader, new NoPaginationRequest())
                        .Items
                        .CountAsync(cancellationToken)
                    : Convert.ToInt32(request.Context
                        .BuildingDetailListCountView
                        .First()
                        .Count)
            };

        }
    }
}
