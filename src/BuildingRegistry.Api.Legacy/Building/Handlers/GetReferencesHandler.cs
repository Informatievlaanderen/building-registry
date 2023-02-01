namespace BuildingRegistry.Api.Legacy.Handlers.Building
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Building;
    using Api.Legacy.Abstractions.Building.Responses;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetReferencesHandler : IRequestHandler<GetReferencesRequest, BuildingReferencesResponse>
    {
        public async Task<BuildingReferencesResponse> Handle(GetReferencesRequest request, CancellationToken cancellationToken)
        {
            var building = await request.Context
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

            var crabMappings = await request.Context.BuildingPersistentIdCrabIdMappings.FindAsync(new object[] { building.BuildingId }, cancellationToken);
            var crabReferences =
                crabMappings.CrabTerrainObjectId.HasValue && !string.IsNullOrEmpty(crabMappings.CrabIdentifierTerrainObject)
                    ? new CrabReferences(crabMappings.CrabTerrainObjectId.Value, crabMappings.CrabIdentifierTerrainObject)
                    : null;

            return new BuildingReferencesResponse(
                building.PersistentLocalId.Value,
                request.ResponseOptions.Value.GebouwNaamruimte,
                building.Version.ToBelgianDateTimeOffset(),
                crabReferences);
        }
    }
}
