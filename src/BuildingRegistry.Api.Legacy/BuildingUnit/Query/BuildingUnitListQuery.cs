namespace BuildingRegistry.Api.Legacy.BuildingUnit.Query
{
    using Be.Vlaanderen.Basisregisters.Api.Search;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Microsoft.EntityFrameworkCore;
    using Projections.Legacy;
    using Projections.Legacy.BuildingUnitDetail;
    using Projections.Syndication;
    using System.Collections.Generic;
    using System.Linq;

    public class BuildingUnitListQuery : Query<BuildingUnitDetailItem, BuildingUnitFilter>
    {
        private readonly LegacyContext _context;
        private readonly SyndicationContext _syndicationContext;

        protected override ISorting Sorting => new BuildingUnitSorting();

        public BuildingUnitListQuery(
            LegacyContext context,
            SyndicationContext syndicationContext)
        {
            _context = context;
            _syndicationContext = syndicationContext;
        }

        protected override IQueryable<BuildingUnitDetailItem> Filter(FilteringHeader<BuildingUnitFilter> filtering)
        {
            var buildingUnits = _context
                .BuildingUnitDetails
                .Where(x => x.IsComplete && !x.IsRemoved && x.IsBuildingComplete)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(filtering.Filter.AddressOsloId))
            {
                var addressOsloIds = _syndicationContext
                    .AddressOsloIds
                    .Where(x => x.OsloId == filtering.Filter.AddressOsloId)
                    .Select(x => x.AddressId)
                    .ToList();

                buildingUnits = _context
                    .BuildingUnitDetails
                    .Where(unit => unit.Addresses.Any(address => addressOsloIds.Contains(address.AddressId)));
            }

            return buildingUnits;
        }
    }

    internal class BuildingUnitSorting : ISorting
    {
        public IEnumerable<string> SortableFields { get; } = new[]
        {
            nameof(BuildingUnitDetailItem.OsloId),
        };

        public SortingHeader DefaultSortingHeader { get; } = new SortingHeader(nameof(BuildingUnitDetailItem.OsloId), SortOrder.Ascending);
    }

    public class BuildingUnitFilter
    {
        public string AddressOsloId { get; set; }
    }
}
