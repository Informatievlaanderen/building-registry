namespace BuildingRegistry.Api.Oslo.Building.Query
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Api.Search;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw;
    using Converters;
    using Microsoft.EntityFrameworkCore;
    using Projections.Legacy;

    public class BuildingListOsloQueryV2 : Query<BuildingQueryItem, BuildingFilter>
    {
        private readonly LegacyContext _context;

        protected override ISorting Sorting => new BuildingSorting();

        public BuildingListOsloQueryV2(LegacyContext context)
        {
            _context = context;
        }

        protected override IQueryable<BuildingQueryItem> Filter(FilteringHeader<BuildingFilter> filtering)
        {
            var buildings = _context
                .BuildingDetailsV2
                .Where(x => !x.IsRemoved)
                .OrderBy(x => x.PersistentLocalId)
                .AsNoTracking();

            if (!filtering.ShouldFilter)
                return buildings.Select(x => new BuildingQueryItem
                {
                    PersistentLocalId = x.PersistentLocalId,
                    StatusAsString = x.StatusAsString,
                    VersionTimestampAsDateTimeOffset = x.VersionTimestampAsDateTimeOffset
                });

            if (!string.IsNullOrEmpty(filtering.Filter.Status))
            {
                if (Enum.TryParse(typeof(GebouwStatus), filtering.Filter.Status, true, out var status))
                {
                    var buildingStatus = ((GebouwStatus)status).MapToV2();
                    buildings = buildings.Where(m => m.StatusAsString == buildingStatus.Value);
                }
                else
                {
                    return new List<BuildingQueryItem>().AsQueryable();
                }
            }

            return buildings.Select(x => new BuildingQueryItem
            {
                PersistentLocalId = x.PersistentLocalId,
                StatusAsString = x.StatusAsString,
                VersionTimestampAsDateTimeOffset = x.VersionTimestampAsDateTimeOffset
            });
        }
    }
}
