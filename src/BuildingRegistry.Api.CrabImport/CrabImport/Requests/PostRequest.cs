namespace BuildingRegistry.Api.CrabImport.Abstractions.Post
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Api;
    using MediatR;

    public record PostRequest(List<RegisterCrabImportRequest[]> RegisterCrabImportList, IDictionary<string, object> Metadata, IdempotentCommandHandlerModule Bus) : IRequest<ConcurrentBag<long?>>;
}
