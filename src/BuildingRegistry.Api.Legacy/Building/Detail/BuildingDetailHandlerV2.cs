namespace BuildingRegistry.Api.Legacy.Building.Detail
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw;
    using BuildingUnit;
    using Consumer.Read.Parcel;
    using Infrastructure.Options;
    using Infrastructure.ParcelMatching;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Projections.Legacy;

    public class GetDetailHandlerV2 : IRequestHandler<GetRequest, BuildingDetailResponseWithEtag>
    {
        private readonly LegacyContext _context;
        private readonly ConsumerParcelContext _consumerParcelContext;
        private readonly IOptions<ResponseOptions> _responseOptions;
        private readonly IParcelMatching _grbBuildingParcel;

        public GetDetailHandlerV2(
            LegacyContext context,
            ConsumerParcelContext consumerParcelContext,
            IOptions<ResponseOptions> responseOptions,
            IParcelMatching grbBuildingParcel)
        {
            _context = context;
            _consumerParcelContext = consumerParcelContext;
            _responseOptions = responseOptions;
            _grbBuildingParcel = grbBuildingParcel;
        }

        public async Task<BuildingDetailResponseWithEtag> Handle(GetRequest request, CancellationToken cancellationToken)
        {
            var building = await _context
                .BuildingDetailsV2
                .AsNoTracking()
                .SingleOrDefaultAsync(item => item.PersistentLocalId == request.PersistentLocalId, cancellationToken);

            if (building is { IsRemoved: true })
            {
                throw new ApiException("Gebouw werd verwijderd.", StatusCodes.Status410Gone);
            }

            if (building is null)
            {
                throw new ApiException("Onbestaand gebouw.", StatusCodes.Status404NotFound);
            }

            var buildingUnitPersistentLocalIdsTask = _context
                .BuildingUnitDetailsV2WithCount
                .Where(x => x.BuildingPersistentLocalId == building.PersistentLocalId)
                .Where(x => !x.IsRemoved)
                .Select(x => new { x.BuildingUnitPersistentLocalId, x.Status })
                .ToListAsync(cancellationToken);

            var parcels = _grbBuildingParcel
                .GetUnderlyingParcels(building.Geometry)
                .Select(s => CaPaKey.CreateFrom(s).VbrCaPaKey)
                .Distinct();

            var caPaKeysTask = _consumerParcelContext
                .ParcelConsumerItemsWithCount
                .Where(x => !x.IsRemoved && parcels.Contains(x.CaPaKey))
                .Select(x => x.CaPaKey)
                .ToListAsync(cancellationToken);

            await Task.WhenAll(buildingUnitPersistentLocalIdsTask, caPaKeysTask);

            var buildingUnitPersistentLocalIds = buildingUnitPersistentLocalIdsTask.Result;
            var caPaKeys = caPaKeysTask.Result;

            return new BuildingDetailResponseWithEtag(
                new BuildingDetailResponse(
                    building.PersistentLocalId,
                    _responseOptions.Value.GebouwNaamruimte,
                    building.Version.ToBelgianDateTimeOffset(),
                    BuildingHelpers.GetBuildingPolygon(building.Geometry),
                    building.GeometryMethod.Map(),
                    building.Status.Map(),
                    buildingUnitPersistentLocalIds
                        .OrderBy(x => x.BuildingUnitPersistentLocalId)
                        .Select(x =>
                            new GebouwDetailGebouweenheid(
                                x.BuildingUnitPersistentLocalId.ToString(),
                                x.Status.Map(),
                                string.Format(_responseOptions.Value.GebouweenheidDetailUrl, x.BuildingUnitPersistentLocalId)))
                        .ToList(),
                    caPaKeys.Select(x =>
                            new GebouwDetailPerceel(x, string.Format(_responseOptions.Value.PerceelUrl, x)))
                        .ToList()),
                building.LastEventHash);
        }
    }
}
