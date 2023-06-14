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

    public class GetBuildingUnitDetailHandlerV2 : IRequestHandler<GetBuildingUnitDetailRequest, BuildingUnitResponseWithEtag>
    {
        private readonly LegacyContext _context;
        private readonly IOptions<ResponseOptions> _responseOptions;

        public GetBuildingUnitDetailHandlerV2(
            LegacyContext context,
            IOptions<ResponseOptions> responseOptions)
        {
            _context = context;
            _responseOptions = responseOptions;
        }

        public async Task<BuildingUnitResponseWithEtag> Handle(GetBuildingUnitDetailRequest buildingUnitDetailRequest, CancellationToken cancellationToken)
        {
            var buildingUnit = await _context
                .BuildingUnitDetailsV2
                .Include(x => x.Addresses)
                .AsNoTracking()
                .SingleOrDefaultAsync(item => item.BuildingUnitPersistentLocalId == buildingUnitDetailRequest.PersistentLocalId, cancellationToken);

            if (buildingUnit is null)
            {
                throw new ApiException("Onbestaande gebouweenheid.", StatusCodes.Status404NotFound);
            }

            if (buildingUnit is { IsRemoved: true })
            {
                throw new ApiException("Gebouweenheid werd verwijderd.", StatusCodes.Status410Gone);
            }

            return new BuildingUnitResponseWithEtag(
                new BuildingUnitResponse(
                    buildingUnit.BuildingUnitPersistentLocalId,
                    _responseOptions.Value.GebouweenheidNaamruimte,
                    buildingUnit.Version.ToBelgianDateTimeOffset(),
                    BuildingUnitHelpers.GetBuildingUnitPoint(buildingUnit.Position),
                    buildingUnit.PositionMethod.Map(),
                    buildingUnit.Status.Map(),
                    buildingUnit.Function.Map(),
                    new GebouweenheidDetailGebouw(
                        buildingUnit.BuildingPersistentLocalId.ToString(),
                        string.Format(_responseOptions.Value.GebouwDetailUrl,
                            buildingUnit.BuildingPersistentLocalId)),
                    buildingUnit.Addresses.Select(x =>
                        new GebouweenheidDetailAdres(
                            x.AddressPersistentLocalId.ToString(),
                            string.Format(_responseOptions.Value.AdresUrl, x.AddressPersistentLocalId.ToString())))
                        .ToList(),
                    buildingUnit.HasDeviation),
                buildingUnit.LastEventHash);
        }
    }
}
