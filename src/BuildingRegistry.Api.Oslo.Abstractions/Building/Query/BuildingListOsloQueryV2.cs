namespace BuildingRegistry.Api.Oslo.Abstractions.Building.Query
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
    using Projections.Legacy.BuildingDetail;
    using Projections.Legacy.BuildingDetailV2;

    public class BuildingListOsloQueryV2 : Query<BuildingDetailItemV2, BuildingFilterV2>
    {
        private readonly LegacyContext _context;

        protected override ISorting Sorting => new BuildingSorting();

        public BuildingListOsloQueryV2(LegacyContext context)
        {
            _context = context;
        }

        protected override IQueryable<BuildingDetailItemV2> Filter(FilteringHeader<BuildingFilterV2> filtering)
        {
            var buildings = _context
                .BuildingDetailsV2
                .Where(x => !x.IsRemoved)
                .OrderBy(x => x.PersistentLocalId)
                .AsNoTracking();

            if (!filtering.ShouldFilter)
                return buildings;

            if (!string.IsNullOrEmpty(filtering.Filter.Status))
            {
                if (Enum.TryParse(typeof(GebouwStatus), filtering.Filter.Status, true, out var status))
                {
                    var buildingStatus = ((GebouwStatus)status).MapToV2();
                    buildings = buildings.Where(m => m.Status == buildingStatus);
                }
                else
                    //have to filter on EF cannot return new List<>().AsQueryable() cause non-EF provider does not support .CountAsync()
                    buildings = new List<BuildingDetailItemV2>().AsQueryable();
                //buildings = buildings.Where(m => m.Status.HasValue && (int)m.Status.Value == -1);
            }

            return buildings;
        }
    }

    public class BuildingSortingV2 : ISorting
    {
        public IEnumerable<string> SortableFields { get; } = new[]
        {
            nameof(BuildingDetailItemV2.PersistentLocalId)
        };

        public SortingHeader DefaultSortingHeader { get; } = new SortingHeader(nameof(BuildingDetailItemV2.PersistentLocalId), SortOrder.Ascending);
    }

    public class BuildingFilterV2
    {
        public string Status { get; set; }
    }
}
