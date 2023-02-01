namespace BuildingRegistry.Api.Legacy.Building.Query
{
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Api.Search;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using BuildingRegistry.Projections.Legacy;
    using BuildingRegistry.Projections.Legacy.BuildingDetail;
    using BuildingRegistry.Projections.Legacy.BuildingPersistentIdCrabIdMapping;
    using Microsoft.EntityFrameworkCore;

    public class BuildingCrabMappingQuery : Query<BuildingPersistentLocalIdCrabIdMapping, BuildingCrabMappingFilter>
    {
        private readonly LegacyContext _context;

        protected override ISorting Sorting => new BuildingCrabMappingSorting();

        public BuildingCrabMappingQuery(LegacyContext context)
        {
            _context = context;
        }

        protected override IQueryable<BuildingPersistentLocalIdCrabIdMapping> Filter(FilteringHeader<BuildingCrabMappingFilter> filtering)
        {
            var buildings = _context
                .BuildingPersistentIdCrabIdMappings
                .Where(x => x.PersistentLocalId.HasValue && x.CrabTerrainObjectId.HasValue && x.CrabIdentifierTerrainObject != null)
                .OrderBy(x => x.PersistentLocalId)
                .AsNoTracking();

            if (filtering.Filter.TerrainObjectId.HasValue)
            {
                buildings = buildings.Where(x => x.CrabTerrainObjectId == filtering.Filter.TerrainObjectId);
            }

            if (!string.IsNullOrEmpty(filtering.Filter.IdentifierTerrainObject))
            {
                buildings = buildings.Where(x => x.CrabIdentifierTerrainObject == filtering.Filter.IdentifierTerrainObject);
            }

            return buildings;
        }
    }

    public class BuildingCrabMappingSorting : ISorting
    {
        public IEnumerable<string> SortableFields { get; } = new[]
        {
            nameof(BuildingDetailItem.PersistentLocalId)
        };

        public SortingHeader DefaultSortingHeader { get; } = new SortingHeader(nameof(BuildingDetailItem.PersistentLocalId), SortOrder.Ascending);
    }

    public class BuildingCrabMappingFilter
    {
        public int? TerrainObjectId { get; set; }
        public string? IdentifierTerrainObject { get; set; }
    }
}
