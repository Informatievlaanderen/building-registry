namespace BuildingRegistry.Api.Oslo.Building.V3.Count
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo;
    using Consumer.Read.Parcel;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Projections.Legacy;
    using Query;

    public class BuildingCountHandler : IRequestHandler<BuildingCountRequest, TotaalAantalResponse>
    {
        private readonly LegacyContext _legacyContext;
        private readonly ConsumerParcelContext _consumerParcelContext;
        private readonly IBuildingMatching _buildingMatching;

        public BuildingCountHandler(
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
                    ? await new BuildingListOsloQuery(_legacyContext, _consumerParcelContext, _buildingMatching)
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
