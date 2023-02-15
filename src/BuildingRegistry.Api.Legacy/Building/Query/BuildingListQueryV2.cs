namespace BuildingRegistry.Api.Legacy.Building.Query
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Api.Search;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw;
    using BuildingRegistry.Projections.Legacy;
    using BuildingRegistry.Projections.Legacy.BuildingDetailV2;
    using Microsoft.EntityFrameworkCore;

    public class BuildingListQueryV2 : Query<BuildingDetailItemV2, BuildingFilter>
    {
        private readonly LegacyContext _context;

        protected override ISorting Sorting => new BuildingSorting();

        public BuildingListQueryV2(LegacyContext context)
        {
            _context = context;
        }

        protected override IQueryable<BuildingDetailItemV2> Filter(FilteringHeader<BuildingFilter> filtering)
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
                    buildings = buildings.Where(m =>  m.Status == buildingStatus);
                }
                else
                    buildings =  new List<BuildingDetailItemV2>().AsQueryable();
            }

            return buildings;
        }
    }

    public class BuildingSortingV2 : ISorting
    {
        public IEnumerable<string> SortableFields { get; } = new[]
        {
            nameof(BuildingDetailItemV2.PersistentLocalId),
        };

        public SortingHeader DefaultSortingHeader { get; } = new SortingHeader(nameof(BuildingDetailItemV2.PersistentLocalId), SortOrder.Ascending);
    }
}
