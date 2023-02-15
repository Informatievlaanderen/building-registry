namespace BuildingRegistry.Api.Legacy.BuildingUnit.Detail
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetBuildingUnitDetailHandler : IRequestHandler<GetBuildingUnitDetailRequest, BuildingUnitResponseWithEtag>
    {
        public async Task<BuildingUnitResponseWithEtag> Handle(GetBuildingUnitDetailRequest buildingUnitDetailRequest, CancellationToken cancellationToken)
        {
            var buildingUnit = await buildingUnitDetailRequest.Context
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
            var addressPersistentLocalIds = await buildingUnitDetailRequest.SyndicationContext
                .AddressPersistentLocalIds
                .Where(x => addressIds.Contains(x.AddressId))
                .Select(x => x.PersistentLocalId)
                .ToListAsync(cancellationToken);

            return new BuildingUnitResponseWithEtag(
                new BuildingUnitResponse(
                    buildingUnit.PersistentLocalId.Value,
                    buildingUnitDetailRequest.ResponseOptions.Value.GebouweenheidNaamruimte,
                    buildingUnit.Version.ToBelgianDateTimeOffset(),
                    BuildingUnitHelpers.GetBuildingUnitPoint(buildingUnit.Position),
                    buildingUnit.PositionMethod.Value.ConvertFromBuildingUnitGeometryMethod(),
                    buildingUnit.Status.Value.ConvertFromBuildingUnitStatus(),
                    buildingUnit.Function.ConvertFromBuildingUnitFunction(),
                    new GebouweenheidDetailGebouw(buildingUnit.BuildingPersistentLocalId.Value.ToString(), string.Format(buildingUnitDetailRequest.ResponseOptions.Value.GebouwDetailUrl, buildingUnit.BuildingPersistentLocalId.Value)),
                    addressPersistentLocalIds.Select(id => new GebouweenheidDetailAdres(id, string.Format(buildingUnitDetailRequest.ResponseOptions.Value.AdresUrl, id))).ToList(),
                    false));
        }
    }
}
