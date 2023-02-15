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

    public class CountHandlerV2 : IRequestHandler<CountRequest, TotaalAantalResponse>
    {
        public async Task<TotaalAantalResponse> Handle(CountRequest request, CancellationToken cancellationToken)
        {
            return new TotaalAantalResponse
            {
                Aantal = request.FilteringHeader.ShouldFilter
                    ? await new BuildingUnitListQueryV2(request.Context)
                        .Fetch(request.FilteringHeader, request.SortingHeader, new NoPaginationRequest())
                        .Items
                        .CountAsync(cancellationToken)
                    : Convert.ToInt32(request.Context
                        .BuildingUnitDetailListCountViewV2
                        .First()
                        .Count)
            };
        }
    }
}
