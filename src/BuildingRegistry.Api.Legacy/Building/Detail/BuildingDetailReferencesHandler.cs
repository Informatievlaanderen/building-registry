namespace BuildingRegistry.Api.Legacy.Building.Detail
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Projections.Legacy;

    public class BuildingDetailReferencesHandler : IRequestHandler<GetReferencesRequest, BuildingDetailReferencesResponse>
    {
        private readonly LegacyContext _context;
        private readonly IOptions<ResponseOptions> _responseOptions;

        public BuildingDetailReferencesHandler(
            LegacyContext context,
            IOptions<ResponseOptions> responseOptions)
        {
            _context = context;
            _responseOptions = responseOptions;
        }

        public async Task<BuildingDetailReferencesResponse> Handle(GetReferencesRequest request, CancellationToken cancellationToken)
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

            var crabMappings = await _context.BuildingPersistentIdCrabIdMappings.FindAsync(new object[] { building.BuildingId }, cancellationToken);
            var crabReferences =
                crabMappings.CrabTerrainObjectId.HasValue && !string.IsNullOrEmpty(crabMappings.CrabIdentifierTerrainObject)
                    ? new CrabReferences(crabMappings.CrabTerrainObjectId.Value, crabMappings.CrabIdentifierTerrainObject)
                    : null;

            return new BuildingDetailReferencesResponse(
                building.PersistentLocalId.Value,
                _responseOptions.Value.GebouwNaamruimte,
                building.Version.ToBelgianDateTimeOffset(),
                crabReferences);
        }
    }
}
