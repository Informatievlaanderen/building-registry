namespace BuildingRegistry.Api.CrabImport.Abstractions.Post
{
    using System.Collections.Generic;

    public record SqsPostRequest(List<RegisterCrabImportRequest[]> RegisterCrabImportList, IDictionary<string, object> Metadata, IdempotentCommandHandlerModule Bus) : IRequest<Unit>;
}
