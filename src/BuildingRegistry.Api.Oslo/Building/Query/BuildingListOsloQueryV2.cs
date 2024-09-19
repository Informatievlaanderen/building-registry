namespace BuildingRegistry.Api.Oslo.Building.Query
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Api.Search;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw;
    using Consumer.Read.Parcel;
    using Consumer.Read.Parcel.ParcelWithCount;
    using Converters;
    using Microsoft.EntityFrameworkCore;
    using Projections.Legacy;
    using Projections.Legacy.BuildingDetailV2;

    public class BuildingListOsloQueryV2 : Query<BuildingQueryItem, BuildingFilter>
    {
        private readonly LegacyContext _context;
        private readonly ConsumerParcelContext _consumerParcelContext;
        private readonly IBuildingMatching _buildingMatching;
        protected override ISorting Sorting => new BuildingSorting();

        public BuildingListOsloQueryV2(
            LegacyContext context,
            ConsumerParcelContext consumerParcelContext,
            IBuildingMatching buildingMatching)
        {
            _context = context;
            _consumerParcelContext = consumerParcelContext;
            _buildingMatching = buildingMatching;
        }

        protected override IQueryable<BuildingQueryItem> Filter(FilteringHeader<BuildingFilter> filtering)
        {
            var buildings = _context
                .BuildingDetailsV2
                .Where(x => !x.IsRemoved)
                .OrderBy(x => x.PersistentLocalId)
                .AsNoTracking();

            if (!filtering.ShouldFilter)
                return buildings.Select(x => new BuildingQueryItem
                {
                    PersistentLocalId = x.PersistentLocalId,
                    StatusAsString = x.StatusAsString,
                    VersionTimestampAsDateTimeOffset = x.VersionTimestampAsDateTimeOffset
                });

            if (!string.IsNullOrEmpty(filtering.Filter.Status))
            {
                if (Enum.TryParse(typeof(GebouwStatus), filtering.Filter.Status, true, out var status))
                {
                    var buildingStatus = ((GebouwStatus)status).MapToV2();
                    buildings = buildings.Where(m => m.StatusAsString == buildingStatus.Value);
                }
                else
                {
                    return new List<BuildingQueryItem>().AsQueryable();
                }
            }

            if (!string.IsNullOrEmpty(filtering.Filter.CaPaKey))
            {
                var parcel = _consumerParcelContext.ParcelConsumerItemsWithCount.FirstOrDefault(x => x.CaPaKey == filtering.Filter.CaPaKey);
                if (parcel is not null && parcel.Status == ParcelStatus.Realized)
                {
                    var underlyingBuildings = _buildingMatching.GetUnderlyingBuildings(parcel.Geometry);
                    buildings = buildings.Where(x => underlyingBuildings.Contains(x.PersistentLocalId));
                }
                else
                {
                    buildings = new List<BuildingDetailItemV2>().AsQueryable();
                }
            }

            return buildings.Select(x => new BuildingQueryItem
            {
                PersistentLocalId = x.PersistentLocalId,
                StatusAsString = x.StatusAsString,
                VersionTimestampAsDateTimeOffset = x.VersionTimestampAsDateTimeOffset
            });
        }
    }

    public class BuildingSorting : ISorting
    {
        public IEnumerable<string> SortableFields { get; } = new[]
        {
            nameof(BuildingDetailItemV2.PersistentLocalId)
        };

        public SortingHeader DefaultSortingHeader { get; } = new SortingHeader(nameof(BuildingDetailItemV2.PersistentLocalId), SortOrder.Ascending);
    }

    public class BuildingFilter
    {
        public string Status { get; set; }
        public string? CaPaKey { get; set; }
    }
}
