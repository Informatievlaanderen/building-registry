namespace BuildingRegistry.Api.Legacy.Handlers.BuildingUnit
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.BuildingUnit.Responses;
    using Abstractions.Converters;
    using Api.Legacy.Abstractions.BuildingUnit;
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
                buildingUnit.PositionMethod.Value.ConvertFromBuildingUnitGeometryMethod(),
                buildingUnit.Status.Value.ConvertFromBuildingUnitStatus(),
                buildingUnit.Function.ConvertFromBuildingUnitFunction(),
                new GebouweenheidDetailGebouw(
                    buildingUnit.PersistentLocalId.Value.ToString(),
                    string.Format(request.ResponseOptions.Value.GebouweenheidDetailUrl,
                        buildingUnit.PersistentLocalId.Value)),
                addressPersistentLocalIds.Select(id => new GebouweenheidDetailAdres(id, string.Format(request.ResponseOptions.Value.AdresUrl, id))).ToList());
        }
    }
}
