namespace BuildingRegistry.Api.Oslo.Handlers.Count
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Projections.Legacy;

    public class CountRequest : IRequest<TotaalAantalResponse>
    {
        public LegacyContext Context { get; }
        public HttpRequest HttpRequest { get; }

        public CountRequest(LegacyContext context, HttpRequest httpRequest)
        {
            Context = context;
            HttpRequest = httpRequest;
        }
    }
}
