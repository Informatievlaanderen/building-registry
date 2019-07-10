namespace BuildingRegistry.Api.Legacy.Building.Query
{
    using Be.Vlaanderen.Basisregisters.Api.Search;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Projections.Legacy;
    using Projections.Legacy.BuildingDetail;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;

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
                .Where(x => x.IsComplete && !x.IsRemoved)
                .AsNoTracking();

            return buildings;
        }
    }

    internal class BuildingSorting : ISorting
    {
        public IEnumerable<string> SortableFields { get; } = new[]
        {
            nameof(BuildingDetailItem.PersistentLocalId),
        };

        public SortingHeader DefaultSortingHeader { get; } = new SortingHeader(nameof(BuildingDetailItem.PersistentLocalId), SortOrder.Ascending);
    }

    public class BuildingFilter { }
}
