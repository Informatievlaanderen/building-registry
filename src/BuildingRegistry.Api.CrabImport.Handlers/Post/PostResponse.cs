namespace BuildingRegistry.Api.CrabImport.Handlers.Post
{
    using System.Collections.Concurrent;

    public class PostResponse
    {
        public ConcurrentBag<long?> Tags { get; set; }

        public PostResponse(ConcurrentBag<long?> tags)
        {
            Tags = tags;
        }
    }
}
