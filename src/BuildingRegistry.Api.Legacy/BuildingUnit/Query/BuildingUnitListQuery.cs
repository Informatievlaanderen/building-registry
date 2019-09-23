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
                .Where(x => x.IsComplete && !x.IsRemoved && x.IsBuildingComplete && x.PersistentLocalId.HasValue && x.BuildingPersistentLocalId.HasValue)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(filtering.Filter?.AddressPersistentLocalId))
            {
                var addressPersistentLocalIds = _syndicationContext
                    .AddressPersistentLocalIds
                    .Where(x => x.PersistentLocalId == filtering.Filter.AddressPersistentLocalId)
                    .Select(x => x.AddressId)
                    .ToList();

                buildingUnits = _context
                    .BuildingUnitDetails
                    .Where(unit => unit.Addresses.Any(address => addressPersistentLocalIds.Contains(address.AddressId)));
            }

            return buildingUnits;
        }
    }

    public class BuildingUnitSorting : ISorting
    {
        public IEnumerable<string> SortableFields { get; } = new[]
        {
            nameof(BuildingUnitDetailItem.PersistentLocalId),
        };

        public SortingHeader DefaultSortingHeader { get; } = new SortingHeader(nameof(BuildingUnitDetailItem.PersistentLocalId), SortOrder.Ascending);
    }

    public class BuildingUnitFilter
    {
        public string AddressPersistentLocalId { get; set; }
    }
}
