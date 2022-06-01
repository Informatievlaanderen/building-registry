namespace BuildingRegistry.Api.Oslo.Handlers.GetList
{
    using Abstractions.Building.Responses;
    using Abstractions.Infrastructure.Options;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;
    using Projections.Legacy;

    public class GetListRequest : IRequest<BuildingListOsloResponse>
    {
        public HttpRequest HttpRequest { get; }
        public HttpResponse HttpResponse { get; }
        public LegacyContext Context { get; }
        public IOptions<ResponseOptions> ResponseOptions { get; }

        public GetListRequest(HttpRequest httpRequest, HttpResponse httpResponse, LegacyContext context, IOptions<ResponseOptions> responseOptions)
        {
            HttpRequest = httpRequest;
            HttpResponse = httpResponse;
            Context = context;
            ResponseOptions = responseOptions;
        }
    }
}
