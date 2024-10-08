namespace BuildingRegistry.Api.Oslo.Building.Count
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Consumer.Read.Parcel;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Projections.Legacy;
    using Query;

    public class BuildingCountHandlerV2 : IRequestHandler<BuildingCountRequest, TotaalAantalResponse>
    {
        private readonly LegacyContext _legacyContext;
        private readonly ConsumerParcelContext _consumerParcelContext;
        private readonly IBuildingMatching _buildingMatching;

        public BuildingCountHandlerV2(
            LegacyContext legacyContext, ConsumerParcelContext consumerParcelContext, IBuildingMatching buildingMatching)
        {
            _legacyContext = legacyContext;
            _consumerParcelContext = consumerParcelContext;
            _buildingMatching = buildingMatching;
        }

        public async Task<TotaalAantalResponse> Handle(BuildingCountRequest request, CancellationToken cancellationToken)
        {
            return new TotaalAantalResponse
            {
                Aantal = request.FilteringHeader.ShouldFilter
                    ? await new BuildingListOsloQueryV2(_legacyContext, _consumerParcelContext, _buildingMatching)
                        .Fetch(request.FilteringHeader, request.SortingHeader, new NoPaginationRequest())
                        .Items
                        .CountAsync(cancellationToken)
                    : Convert.ToInt32(_legacyContext
                        .BuildingDetailListCountViewV2
                        .First()
                        .Count)
            };

        }
    }
}
