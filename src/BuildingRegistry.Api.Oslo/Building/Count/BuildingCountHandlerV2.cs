namespace BuildingRegistry.Api.Oslo.Building.Count
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using BuildingRegistry.Api.Oslo.Building.Query;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class BuildingCountHandlerV2 : IRequestHandler<BuildingCountRequest, TotaalAantalResponse>
    {
        public async Task<TotaalAantalResponse> Handle(BuildingCountRequest request, CancellationToken cancellationToken)
        {
            return new TotaalAantalResponse
            {
                Aantal = request.FilteringHeader.ShouldFilter
                    ? await new BuildingListOsloQueryV2(request.Context)
                        .Fetch(request.FilteringHeader, request.SortingHeader, new NoPaginationRequest())
                        .Items
                        .CountAsync(cancellationToken)
                    : Convert.ToInt32(request.Context
                        .BuildingDetailListCountViewV2
                        .First()
                        .Count)
            };

        }
    }
}
