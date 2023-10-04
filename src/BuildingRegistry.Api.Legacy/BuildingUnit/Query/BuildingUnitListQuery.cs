namespace BuildingRegistry.Api.Legacy.BuildingUnit.Query
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Api.Search;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid;
    using BuildingRegistry.Projections.Legacy;
    using BuildingRegistry.Projections.Legacy.BuildingUnitDetail;
    using BuildingRegistry.Projections.Syndication;
    using Microsoft.EntityFrameworkCore;

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
                .OrderBy(x => x.PersistentLocalId)
                .AsNoTracking();

            if (!filtering.ShouldFilter)
                return buildingUnits;

            if (!string.IsNullOrEmpty(filtering.Filter?.AddressPersistentLocalId))
            {
                var addressPersistentLocalIds = _syndicationContext
                    .AddressPersistentLocalIds
                    .Where(x => x.PersistentLocalId == filtering.Filter.AddressPersistentLocalId)
                    .Select(x => x.AddressId)
                    .ToList();

                buildingUnits = buildingUnits
                    .Where(unit => unit.Addresses.Any(address => addressPersistentLocalIds.Contains(address.AddressId)));
            }

            if (filtering.Filter?.BuildingPersistentLocalId is not null)
            {
                buildingUnits = buildingUnits
                    .Where(unit => unit.BuildingPersistentLocalId == filtering.Filter.BuildingPersistentLocalId.Value);
            }

            if (!string.IsNullOrEmpty(filtering.Filter.Status))
            {
                if (Enum.TryParse(typeof(GebouweenheidStatus), filtering.Filter.Status, true, out var status))
                {
                    var buildingUnitStatus = ((GebouweenheidStatus)status).ConvertFromGebouweenheidStatus();
                    buildingUnits = buildingUnits.Where(m => m.StatusAsString == buildingUnitStatus.Status);
                }
                else
                    //have to filter on EF cannot return new List<>().AsQueryable() cause non-EF provider does not support .CountAsync()
                    buildingUnits = buildingUnits.Where(m => m.StatusAsString == "-1");
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
        public int? BuildingPersistentLocalId { get; set; }
        public string AddressPersistentLocalId { get; set; }
        public string Status { get; set; }
    }
}
