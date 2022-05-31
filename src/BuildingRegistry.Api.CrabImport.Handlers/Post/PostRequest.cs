namespace BuildingRegistry.Api.CrabImport.Handlers.Post
{
    using System.Collections.Generic;
    using Abstractions.Post;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Api;
    using MediatR;

    public class PostRequest : IRequest<PostResponse>
    {
        public List<RegisterCrabImportRequest[]> RegisterCrabImportList { get; }
        public IDictionary<string, object> Metadata { get; }
        public IdempotentCommandHandlerModule Bus { get; }

        public PostRequest(List<RegisterCrabImportRequest[]> registerCrabImportList, IDictionary<string, object> metadata, IdempotentCommandHandlerModule bus)
        {
            RegisterCrabImportList = registerCrabImportList;
            Metadata = metadata;
            Bus = bus;
        }
    }
}
