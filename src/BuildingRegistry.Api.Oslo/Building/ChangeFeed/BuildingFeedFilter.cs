namespace BuildingRegistry.Api.Oslo.Building.ChangeFeed
{
    public sealed class BuildingFeedFilter
    {
        public int? Page { get; set; }
        public long? FeedPosition { get; set; }
    }

    public sealed class BuildingPositionFilter
    {
        public long? Download { get; set; }
        public long? Sync { get; set; }
        public long? ChangeFeedId { get; set; }

        public bool HasMoreThanOneFilter =>
            (Download.HasValue ? 1 : 0)
            + (Sync.HasValue ? 1 : 0)
            + (ChangeFeedId.HasValue ? 1 : 0) > 1;
    }
}
