namespace BuildingRegistry.Api.Legacy.Handlers.BuildingUnitV2
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.BuildingUnit;
    using Abstractions.BuildingUnit.Query;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class CountHandler : IRequestHandler<CountRequest, TotaalAantalResponse>
    {
        public async Task<TotaalAantalResponse> Handle(CountRequest request, CancellationToken cancellationToken)
        {
            var filtering = request.HttpRequest.ExtractFilteringRequest<BuildingUnitFilter>();
            var sorting = request.HttpRequest.ExtractSortingRequest();
            var pagination = new NoPaginationRequest();
            return new TotaalAantalResponse
            {
                Aantal = filtering.ShouldFilter
                    ? await new BuildingUnitListQuery(request.Context, request.SyndicationContext)
                        .Fetch(filtering, sorting, pagination)
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
