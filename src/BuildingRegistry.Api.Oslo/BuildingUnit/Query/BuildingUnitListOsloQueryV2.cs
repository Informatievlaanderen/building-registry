namespace BuildingRegistry.Api.Oslo.BuildingUnit.Query
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Api.Search;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid;
    using Converters;
    using Microsoft.EntityFrameworkCore;
    using Projections.Legacy;
    using Projections.Legacy.BuildingUnitDetailV2;

    public class BuildingUnitListOsloQueryV2 : Query<BuildingUnitQueryItem, BuildingUnitFilter>
    {
        private readonly LegacyContext _context;

        protected override ISorting Sorting => new BuildingUnitSortingV2();

        public BuildingUnitListOsloQueryV2(LegacyContext context)
        {
            _context = context;
        }

        protected override IQueryable<BuildingUnitQueryItem> Filter(FilteringHeader<BuildingUnitFilter> filtering)
        {
            var buildingUnits = _context
                .BuildingUnitDetailsV2
                .Where(x => !x.IsRemoved)
                .OrderBy(x => x.BuildingPersistentLocalId)
                .AsNoTracking();

            if (!filtering.ShouldFilter)
            {
                return buildingUnits.Select(x => new BuildingUnitQueryItem
                {
                    BuildingUnitPersistentLocalId = x.BuildingUnitPersistentLocalId,
                    StatusAsString = x.StatusAsString,
                    VersionTimestampAsDateTimeOffset = x.VersionTimestampAsDateTimeOffset
                });
            }

            if (!string.IsNullOrEmpty(filtering.Filter?.AddressPersistentLocalId))
            {
                if (int.TryParse(filtering.Filter.AddressPersistentLocalId, out var addressPersistentLocalId))
                {
                    buildingUnits = buildingUnits
                            .Where(unit => unit.Addresses.Any(address => address.AddressPersistentLocalId == addressPersistentLocalId));
                }
                else
                {
                    return new List<BuildingUnitQueryItem>().AsQueryable();
                }
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
                    var buildingUnitStatus = ((GebouweenheidStatus)status).Map();
                    buildingUnits = buildingUnits.Where(m => m.StatusAsString == buildingUnitStatus.Status);
                }
                else
                {
                    return new List<BuildingUnitQueryItem>().AsQueryable();
                }
            }

            if (!string.IsNullOrEmpty(filtering.Filter.Functie))
            {
                if (Enum.TryParse(typeof(GebouweenheidFunctie), filtering.Filter.Functie, true, out var functie))
                {
                    var buildingUnitFunction = ((GebouweenheidFunctie)functie).Map();
                    buildingUnits = buildingUnits.Where(m => m.FunctionAsString == buildingUnitFunction.Function);
                }
                else
                {
                    return new List<BuildingUnitQueryItem>().AsQueryable();
                }
            }

            return buildingUnits.Select(x => new BuildingUnitQueryItem
            {
                BuildingUnitPersistentLocalId = x.BuildingUnitPersistentLocalId,
                StatusAsString = x.StatusAsString,
                VersionTimestampAsDateTimeOffset = x.VersionTimestampAsDateTimeOffset
            });
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
}
