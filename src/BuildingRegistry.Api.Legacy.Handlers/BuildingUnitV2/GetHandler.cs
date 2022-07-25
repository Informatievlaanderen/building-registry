namespace BuildingRegistry.Api.Legacy.Handlers.BuildingUnitV2
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.BuildingUnit;
    using Abstractions.BuildingUnit.Responses;
    using Abstractions.Converters;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetHandler : IRequestHandler<GetRequest, BuildingUnitResponse>
    {
        public async Task<BuildingUnitResponse> Handle(GetRequest request, CancellationToken cancellationToken)
        {
            var buildingUnit = await request.Context
                .BuildingUnitDetailsV2
                .Include(x => x.Addresses)
                .AsNoTracking()
                .SingleOrDefaultAsync(item => item.BuildingUnitPersistentLocalId == request.PersistentLocalId, cancellationToken);

            if (buildingUnit is { IsRemoved: true })
            {
                throw new ApiException("Gebouweenheid werd verwijderd.", StatusCodes.Status410Gone);
            }

            if (buildingUnit is null)
            {
                throw new ApiException("Onbestaande gebouweenheid.", StatusCodes.Status404NotFound);
            }

            return new BuildingUnitResponse(
                buildingUnit.BuildingUnitPersistentLocalId,
                request.ResponseOptions.Value.GebouweenheidNaamruimte,
                buildingUnit.Version.ToBelgianDateTimeOffset(),
                BuildingUnitHelpers.GetBuildingUnitPoint(buildingUnit.Position),
                buildingUnit.PositionMethod.Map(),
                buildingUnit.Status.Map(),
                buildingUnit.Function.Map(),
                new GebouweenheidDetailGebouw(
                    buildingUnit.BuildingPersistentLocalId.ToString(),
                    string.Format(request.ResponseOptions.Value.GebouwDetailUrl,
                        buildingUnit.BuildingPersistentLocalId)),
                buildingUnit.Addresses.Select(x =>
                    new GebouweenheidDetailAdres(
                        x.BuildingUnitPersistentLocalId.ToString(),
                        string.Format(request.ResponseOptions.Value.AdresUrl, x.BuildingUnitPersistentLocalId.ToString())))
                    .ToList());
        }
    }
}
