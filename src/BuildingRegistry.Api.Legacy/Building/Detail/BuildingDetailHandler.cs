namespace BuildingRegistry.Api.Legacy.Building.Detail
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw;
    using Infrastructure.Options;
    using Infrastructure.ParcelMatching;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Projections.Legacy;
    using Projections.Syndication;

    public class GetDetailHandler : IRequestHandler<GetRequest, BuildingDetailResponseWithEtag>
    {
        private readonly LegacyContext _context;
        private readonly SyndicationContext _syndicationContext;
        private readonly IOptions<ResponseOptions> _responseOptions;
        private readonly IParcelMatching _grbBuildingParcel;

        public GetDetailHandler(
            LegacyContext context,
            SyndicationContext syndicationContext,
            IOptions<ResponseOptions> responseOptions,
            IParcelMatching grbBuildingParcel)
        {
            _context = context;
            _syndicationContext = syndicationContext;
            _responseOptions = responseOptions;
            _grbBuildingParcel = grbBuildingParcel;
        }

        public async Task<BuildingDetailResponseWithEtag> Handle(GetRequest request, CancellationToken cancellationToken)
        {
            var building = await _context
                .BuildingDetails
                .AsNoTracking()
                .SingleOrDefaultAsync(item => item.PersistentLocalId == request.PersistentLocalId, cancellationToken);

            if (building is { IsRemoved: true })
            {
                throw new ApiException("Gebouw werd verwijderd.", StatusCodes.Status410Gone);
            }

            if (building is not { IsComplete: true })
            {
                throw new ApiException("Onbestaand gebouw.", StatusCodes.Status404NotFound);
            }

            //TODO: improvement getting buildingunits and parcels in parallel.
            var buildingUnits = await _context
                .BuildingUnitDetails
                .Where(x => x.BuildingId == building.BuildingId)
                .Where(x => x.IsComplete && !x.IsRemoved)
                .Select(x => x.PersistentLocalId)
                .ToListAsync(cancellationToken);

            var parcels = _grbBuildingParcel
                .GetUnderlyingParcels(building.Geometry)
                .Select(s => CaPaKey.CreateFrom(s).VbrCaPaKey)
                .Distinct();

            var caPaKeys = await _syndicationContext
                .BuildingParcelLatestItems
                .Where(x => !x.IsRemoved && parcels.Contains(x.CaPaKey))
                .Select(x => x.CaPaKey)
                .ToListAsync(cancellationToken);

            return new BuildingDetailResponseWithEtag(
                new BuildingDetailResponse(
                    building.PersistentLocalId.Value,
                    _responseOptions.Value.GebouwNaamruimte,
                    building.Version.ToBelgianDateTimeOffset(),
                    BuildingHelpers.GetBuildingPolygon(building.Geometry),
                    building.GeometryMethod.Value.ConvertFromBuildingGeometryMethod(),
                    building.Status.Value.ConvertFromBuildingStatus(),
                    buildingUnits.OrderBy(x => x.Value).Select(x => new GebouwDetailGebouweenheid(x.ToString(), string.Format(_responseOptions.Value.GebouweenheidDetailUrl, x))).ToList(),
                    caPaKeys.Select(x => new GebouwDetailPerceel(x, string.Format(_responseOptions.Value.PerceelUrl, x))).ToList()));
        }
    }
}
