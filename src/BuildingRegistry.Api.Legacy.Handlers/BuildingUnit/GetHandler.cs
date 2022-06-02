namespace BuildingRegistry.Api.Legacy.Handlers.BuildingUnit
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.BuildingUnit.Responses;
    using Api.Legacy.Abstractions.BuildingUnit;
    using Api.Legacy.Abstractions.Infrastructure.Options;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid;
    using Projections.Legacy;
    using Projections.Syndication;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;

    public record GetRequest(LegacyContext Context, SyndicationContext SyndicationContext, IOptions<ResponseOptions> ResponseOptions, int PersistentLocalId) : IRequest<BuildingUnitResponse>;

    public class GetHandler : IRequestHandler<GetRequest, BuildingUnitResponse>
    {
        public async Task<BuildingUnitResponse> Handle(GetRequest request, CancellationToken cancellationToken)
        {
            var buildingUnit = await request.Context
                .BuildingUnitDetails
                .Include(x => x.Addresses)
                .AsNoTracking()
                .SingleOrDefaultAsync(item => item.PersistentLocalId == request.PersistentLocalId, cancellationToken);

            if (buildingUnit is { IsRemoved: true })
            {
                throw new ApiException("Gebouweenheid werd verwijderd.", StatusCodes.Status410Gone);
            }

            if (buildingUnit is not { IsComplete: true, IsBuildingComplete: true })
            {
                throw new ApiException("Onbestaande gebouweenheid.", StatusCodes.Status404NotFound);
            }

            var addressIds = buildingUnit.Addresses.Select(x => x.AddressId).ToList();
            var addressPersistentLocalIds = await request.SyndicationContext
                .AddressPersistentLocalIds
                .Where(x => addressIds.Contains(x.AddressId))
                .Select(x => x.PersistentLocalId)
                .ToListAsync(cancellationToken);

            return new BuildingUnitResponse(
                buildingUnit.PersistentLocalId.Value,
                request.ResponseOptions.Value.GebouweenheidNaamruimte,
                buildingUnit.Version.ToBelgianDateTimeOffset(),
                BuildingUnitHelpers.GetBuildingUnitPoint(buildingUnit.Position),
                BuildingUnitHelpers.MapBuildingUnitGeometryMethod(buildingUnit.PositionMethod.Value),
                BuildingUnitHelpers.MapBuildingUnitStatus(buildingUnit.Status.Value),
                BuildingUnitHelpers.MapBuildingUnitFunction(buildingUnit.Function),
                new GebouweenheidDetailGebouw(buildingUnit.BuildingPersistentLocalId.Value.ToString(), string.Format(request.ResponseOptions.Value.GebouwDetailUrl, buildingUnit.BuildingPersistentLocalId.Value)),
                addressPersistentLocalIds.Select(id => new GebouweenheidDetailAdres(id, string.Format(request.ResponseOptions.Value.AdresUrl, id))).ToList());
        }
    }
}
