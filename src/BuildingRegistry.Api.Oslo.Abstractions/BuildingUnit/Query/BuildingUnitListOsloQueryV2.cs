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
    using Converters;
    using Microsoft.EntityFrameworkCore;
    using Projections.Legacy.BuildingUnitDetailV2;
    using Projections.Syndication;

    public class BuildingUnitListOsloQueryV2 : Query<BuildingUnitDetailItemV2, BuildingUnitFilterV2>
    {
        private readonly LegacyContext _context;
        private readonly SyndicationContext _syndicationContext;

        protected override ISorting Sorting => new BuildingUnitSortingV2();

        public BuildingUnitListOsloQueryV2(
            LegacyContext context,
            SyndicationContext syndicationContext)
        {
            _context = context;
            _syndicationContext = syndicationContext;
        }

        protected override IQueryable<BuildingUnitDetailItemV2> Filter(FilteringHeader<BuildingUnitFilterV2> filtering)
        {
            var buildingUnits = _context
                .BuildingUnitDetailsV2
                .Where(x => !x.IsRemoved)
                .OrderBy(x => x.BuildingPersistentLocalId)
                .AsNoTracking();

            if (!filtering.ShouldFilter)
                return buildingUnits;

            if (!string.IsNullOrEmpty(filtering.Filter?.AddressPersistentLocalId))
            {
                var addressPersistentLocalIds = _syndicationContext
                    .AddressPersistentLocalIds
                    .Where(x => x.PersistentLocalId == filtering.Filter.AddressPersistentLocalId)
                    .Select(x => x.PersistentLocalId)
                    .ToList();

                buildingUnits = _context
                    .BuildingUnitDetailsV2
                    .Where(unit => unit.Addresses.Any(address => addressPersistentLocalIds.Contains(address.AddressPersistentLocalId.ToString())));
            }

            if (!string.IsNullOrEmpty(filtering.Filter.Status))
            {
                if (Enum.TryParse(typeof(GebouweenheidStatus), filtering.Filter.Status, true, out var status))
                {
                    var buildingUnitStatus = ((GebouweenheidStatus)status).Map();
                    buildingUnits = buildingUnits.Where(m => m.Status == buildingUnitStatus.Status);
                }
                else
                    //have to filter on EF cannot return new List<>().AsQueryable() cause non-EF provider does not support .CountAsync()
                    buildingUnits = new List<BuildingUnitDetailItemV2>().AsQueryable();
                //buildingUnits = buildingUnits.Where(m => m.Status == "-1");
            }

            return buildingUnits;
        }
    }

    public class BuildingUnitSortingV2 : ISorting
    {
        public IEnumerable<string> SortableFields { get; } = new[]
        {
            // todo: V1 uses PersistentLocalId as sorting field, if a client sends PersistentLocalId as sort it will use the default sortfield
            nameof(BuildingUnitDetailItemV2.BuildingUnitPersistentLocalId),
        };

        public SortingHeader DefaultSortingHeader { get; } = new SortingHeader(nameof(BuildingUnitDetailItemV2.BuildingUnitPersistentLocalId), SortOrder.Ascending);
    }

    public class BuildingUnitFilterV2
    {
        public string AddressPersistentLocalId { get; set; }
        public string Status { get; set; }
    }
}
