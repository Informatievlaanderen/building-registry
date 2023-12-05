namespace BuildingRegistry.Api.Oslo.Building.Query
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Projections.Legacy.BuildingDetail;

    public class BuildingSorting : ISorting
    {
        public IEnumerable<string> SortableFields { get; } = new[]
        {
            nameof(BuildingDetailItem.PersistentLocalId)
        };

        public SortingHeader DefaultSortingHeader { get; } = new SortingHeader(nameof(BuildingDetailItem.PersistentLocalId), SortOrder.Ascending);
    }

    public class BuildingFilter
    {
        public string Status { get; set; }
        public string? CaPaKey { get; set; }
    }
}
