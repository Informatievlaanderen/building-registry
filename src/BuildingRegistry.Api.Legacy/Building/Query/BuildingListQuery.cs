namespace BuildingRegistry.Api.Legacy.Building.Query
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Api.Search;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw;
    using Microsoft.EntityFrameworkCore;
    using Projections.Legacy;
    using Projections.Legacy.BuildingDetail;

    public class BuildingListQuery : Query<BuildingDetailItem, BuildingFilter>
    {
        private readonly LegacyContext _context;
        protected override ISorting Sorting => new BuildingSorting();

        public BuildingListQuery(LegacyContext context)
        {
            _context = context;
        }

        protected override IQueryable<BuildingDetailItem> Filter(FilteringHeader<BuildingFilter> filtering)
        {
            var buildings = _context
                .BuildingDetails
                .Where(x => x.IsComplete && !x.IsRemoved && x.PersistentLocalId.HasValue)
                .OrderBy(x => x.PersistentLocalId)
                .AsNoTracking();

            if (!filtering.ShouldFilter)
                return buildings;

            if (!string.IsNullOrEmpty(filtering.Filter.Status))
            {
                if (Enum.TryParse(typeof(GebouwStatus), filtering.Filter.Status, true, out var status))
                {
                    var buildingStatus = ((GebouwStatus)status).ConvertFromGebouwStatus();
                    buildings = buildings.Where(m => m.Status.HasValue && m.Status.Value == buildingStatus);
                }
                else
                    //have to filter on EF cannot return new List<>().AsQueryable() cause non-EF provider does not support .CountAsync()
                    buildings = buildings.Where(m => m.Status.HasValue && (int)m.Status.Value == -1);
            }

            return buildings;
        }
    }

    public class BuildingSorting : ISorting
    {
        public IEnumerable<string> SortableFields { get; } = new[]
        {
            nameof(BuildingDetailItem.PersistentLocalId),
        };

        public SortingHeader DefaultSortingHeader { get; } = new SortingHeader(nameof(BuildingDetailItem.PersistentLocalId), SortOrder.Ascending);
    }

    public class BuildingFilter
    {
        public string Status { get; set; }
    }
}
