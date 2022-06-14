namespace BuildingRegistry.Api.CrabImport.Abstractions.Post
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    public record PostRequest(List<RegisterCrabImportRequest[]> RegisterCrabImportList, IDictionary<string, object> Metadata, IdempotentCommandHandlerModule Bus) : IRequest<ConcurrentBag<long?>>;
}
