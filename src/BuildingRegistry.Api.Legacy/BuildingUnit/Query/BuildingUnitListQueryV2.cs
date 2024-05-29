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
    using BuildingRegistry.Projections.Legacy.BuildingUnitDetailV2WithCount;
    using Microsoft.EntityFrameworkCore;

    public class BuildingUnitListQueryV2 : Query<BuildingUnitDetailItemV2, BuildingUnitFilter>
    {
        private readonly LegacyContext _context;

        protected override ISorting Sorting => new BuildingUnitSortingV2();

        public BuildingUnitListQueryV2(LegacyContext context)
        {
            _context = context;
        }

        protected override IQueryable<BuildingUnitDetailItemV2> Filter(FilteringHeader<BuildingUnitFilter> filtering)
        {
            var buildingUnits = _context
                .BuildingUnitDetailsV2WithCount
                .Where(x => !x.IsRemoved)
                .OrderBy(x => x.BuildingUnitPersistentLocalId)
                .AsNoTracking();

            if (!filtering.ShouldFilter)
            {
                return buildingUnits;
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
                    return new List<BuildingUnitDetailItemV2>().AsQueryable();
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
                    var buildingUnitStatus = ((GebouweenheidStatus) status).ConvertFromGebouweenheidStatus();
                    buildingUnits = buildingUnits.Where(m => m.StatusAsString == buildingUnitStatus.Status);
                }
                else
                {
                    return new List<BuildingUnitDetailItemV2>().AsQueryable();
                }
            }

            if (!string.IsNullOrEmpty(filtering.Filter.Functie))
            {
                if (Enum.TryParse(typeof(GebouweenheidFunctie), filtering.Filter.Functie, true, out var functie))
                {
                    var buildingUnitFunction = ((GebouweenheidFunctie) functie).Map();
                    buildingUnits = buildingUnits.Where(m => m.FunctionAsString == buildingUnitFunction.Function);
                }
                else
                {
                    return new List<BuildingUnitDetailItemV2>().AsQueryable();
                }
            }

            return buildingUnits;
        }
    }

    public class BuildingUnitSortingV2 : ISorting
    {
        public IEnumerable<string> SortableFields { get; } = new[]
        {
            // todo: V1 uses PersistentLocalId as sorting field, if a client sends PersistentLocalId as sort it will use the default sortfield
            nameof(BuildingUnitDetailItemV2.BuildingPersistentLocalId),
        };

        public SortingHeader DefaultSortingHeader { get; } = new SortingHeader(nameof(BuildingUnitDetailItemV2.BuildingUnitPersistentLocalId), SortOrder.Ascending);
    }
}
