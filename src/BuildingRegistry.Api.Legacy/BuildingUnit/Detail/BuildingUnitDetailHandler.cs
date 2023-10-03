namespace BuildingRegistry.Api.Legacy.BuildingUnit.Detail
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Projections.Legacy;
    using Projections.Syndication;

    public class GetBuildingUnitDetailHandler : IRequestHandler<GetBuildingUnitDetailRequest, BuildingUnitResponseWithEtag>
    {
        private readonly LegacyContext _context;
        private readonly SyndicationContext _syndicationContext;
        private readonly IOptions<ResponseOptions> _responseOptions;

        public GetBuildingUnitDetailHandler(
            LegacyContext context,
            SyndicationContext syndicationContext,
            IOptions<ResponseOptions> responseOptions)
        {
            _context = context;
            _syndicationContext = syndicationContext;
            _responseOptions = responseOptions;
        }

        public async Task<BuildingUnitResponseWithEtag> Handle(GetBuildingUnitDetailRequest buildingUnitDetailRequest,
            CancellationToken cancellationToken)
        {
            var buildingUnit = await _context
                .BuildingUnitDetails
                .Include(x => x.Addresses)
                .AsNoTracking()
                .SingleOrDefaultAsync(item => item.PersistentLocalId == buildingUnitDetailRequest.PersistentLocalId, cancellationToken);

            if (buildingUnit is { IsRemoved: true })
            {
                throw new ApiException("Gebouweenheid werd verwijderd.", StatusCodes.Status410Gone);
            }

            if (buildingUnit is not { IsComplete: true, IsBuildingComplete: true })
            {
                throw new ApiException("Onbestaande gebouweenheid.", StatusCodes.Status404NotFound);
            }

            var addressIds = buildingUnit.Addresses.Select(x => x.AddressId).ToList();
            var addressPersistentLocalIds = await _syndicationContext
                .AddressPersistentLocalIds
                .Where(x => addressIds.Contains(x.AddressId))
                .Select(x => x.PersistentLocalId)
                .ToListAsync(cancellationToken);

            return new BuildingUnitResponseWithEtag(
                new BuildingUnitResponse(
                    buildingUnit.PersistentLocalId.Value,
                    _responseOptions.Value.GebouweenheidNaamruimte,
                    buildingUnit.Version.ToBelgianDateTimeOffset(),
                    BuildingUnitHelpers.GetBuildingUnitPoint(buildingUnit.Position),
                    buildingUnit.PositionMethod.Value.ConvertFromBuildingUnitGeometryMethod(),
                    buildingUnit.Status.Value.ConvertFromBuildingUnitStatus(),
                    buildingUnit.Function.ConvertFromBuildingUnitFunction(),
                    new GebouweenheidDetailGebouw(
                        buildingUnit.BuildingPersistentLocalId.Value.ToString(),
                        string.Format(_responseOptions.Value.GebouwDetailUrl, buildingUnit.BuildingPersistentLocalId.Value)),
                    addressPersistentLocalIds.Select(id => new GebouweenheidDetailAdres(id, string.Format(_responseOptions.Value.AdresUrl, id)))
                        .ToList(),
                    false));
        }
    }
}
