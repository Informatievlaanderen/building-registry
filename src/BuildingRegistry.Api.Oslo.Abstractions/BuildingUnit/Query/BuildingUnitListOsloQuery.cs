namespace BuildingRegistry.Api.Oslo.Abstractions.BuildingUnit.Query
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
    using Converters;
    using Microsoft.EntityFrameworkCore;
    using Projections.Syndication;

    public class BuildingUnitListOsloQuery : Query<BuildingUnitDetailItem, BuildingUnitFilter>
    {
        private readonly LegacyContext _context;
        private readonly SyndicationContext _syndicationContext;

        protected override ISorting Sorting => new BuildingUnitSorting();

        public BuildingUnitListOsloQuery(
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

                buildingUnits = _context
                    .BuildingUnitDetails
                    .Where(unit => unit.Addresses.Any(address => addressPersistentLocalIds.Contains(address.AddressId)));
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
        public string AddressPersistentLocalId { get; set; }
        public string Status { get; set; }
    }
}
