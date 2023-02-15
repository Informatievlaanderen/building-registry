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

    public class GetBuildingUnitDetailHandlerV2 : IRequestHandler<GetBuildingUnitDetailRequest, BuildingUnitResponseWithEtag>
    {
        public async Task<BuildingUnitResponseWithEtag> Handle(GetBuildingUnitDetailRequest buildingUnitDetailRequest, CancellationToken cancellationToken)
        {
            var buildingUnit = await buildingUnitDetailRequest.Context
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
                    buildingUnitDetailRequest.ResponseOptions.Value.GebouweenheidNaamruimte,
                    buildingUnit.Version.ToBelgianDateTimeOffset(),
                    BuildingUnitHelpers.GetBuildingUnitPoint(buildingUnit.Position),
                    buildingUnit.PositionMethod.Map(),
                    buildingUnit.Status.Map(),
                    buildingUnit.Function.Map(),
                    new GebouweenheidDetailGebouw(
                        buildingUnit.BuildingPersistentLocalId.ToString(),
                        string.Format(buildingUnitDetailRequest.ResponseOptions.Value.GebouwDetailUrl,
                            buildingUnit.BuildingPersistentLocalId)),
                    buildingUnit.Addresses.Select(x =>
                        new GebouweenheidDetailAdres(
                            x.AddressPersistentLocalId.ToString(),
                            string.Format(buildingUnitDetailRequest.ResponseOptions.Value.AdresUrl, x.AddressPersistentLocalId.ToString())))
                        .ToList(),
                    buildingUnit.HasDeviation),
                buildingUnit.LastEventHash);
        }
    }
}
