namespace BuildingRegistry.Api.Legacy.Building.Query
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Projections.Legacy.BuildingDetailV2;

    public class BuildingSorting : ISorting
    {
        public IEnumerable<string> SortableFields { get; } = new[]
        {
            nameof(BuildingDetailItemV2.PersistentLocalId),
        };

        public SortingHeader DefaultSortingHeader { get; } = new SortingHeader(nameof(BuildingDetailItemV2.PersistentLocalId), SortOrder.Ascending);
    }

    public class BuildingFilter
    {
        public string Status { get; set; }
    }
}
