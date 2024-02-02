﻿namespace BuildingRegistry.Projector.Infrastructure
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.LastChangedList;
    using BuildingRegistry.Projections.Legacy;
    using Microsoft.EntityFrameworkCore;

    public sealed class LastChangedListBuildingUnitCacheValidator : ICacheValidator
    {
        private readonly LegacyContext _legacyContext;
        private readonly string _projectionName;

        public LastChangedListBuildingUnitCacheValidator(LegacyContext legacyContext, string projectionName)
        {
            _legacyContext = legacyContext;
            _projectionName = projectionName;
        }

        public async Task<bool> CanCache(long position, CancellationToken ct)
        {
            var projectionPosition = await _legacyContext
                .ProjectionStates
                .AsNoTracking()
                .Where(ps => ps.Name == _projectionName)
                .Select(ps => ps.Position)
                .FirstOrDefaultAsync(ct);

            return projectionPosition >= position;
        }
    }
}
